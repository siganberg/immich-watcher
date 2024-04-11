using System.Text;
using CliWrap;
using CliWrap.Buffered;
using Serilog;

var sourcePath = Environment.GetEnvironmentVariable("/data");
var host =  Environment.GetEnvironmentVariable("IMMICH_HOST");
var apiKey = Environment.GetEnvironmentVariable("IMMICH_API_KEY");

var extensionFiles = new[] { ".jpg", ".mp4" };


if (string.IsNullOrWhiteSpace(sourcePath))
{
    Log.Information("IMMICH_WATCHER_PATH is missing. Terminating");
    return;
}

if (!Directory.Exists(sourcePath))
{
    Log.Information("IMMICH_WATCHER_PATH with value {0} doesn't exit. Terminating", sourcePath);
    return;
}

if (string.IsNullOrWhiteSpace(host))
{
    Log.Information("IMMICH_HOST is missing. Terminating");
    return;
}

if (string.IsNullOrWhiteSpace(apiKey))
{
    Log.Information("IMMICH_API_KEY is missing. Terminating");
    return;
}


Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

CreateFolder(Path.Combine(sourcePath, "pending"));
CreateFolder(Path.Combine(sourcePath, "uploaded"));

Log.Information("Started monitoring sources: {Source} folder monitoring started. Press Ctrl+C to exit.", sourcePath);

LoginToImmich(host, apiKey);

while (true)
{
    StartTransferringFiles(sourcePath);
    Thread.Sleep(1000);
}

void StartTransferringFiles(string source)
{
    try
    {
        var files = Directory.EnumerateFiles(Path.Combine(source, "pending"), "*.*", SearchOption.AllDirectories)
            .Where(a =>
            {
                var lower = Path.GetFileName(a).ToLower();
                return extensionFiles.Any(b => b == Path.GetExtension(lower));
            });

        var fileDetectedMessage = false;



        var stdOutBuffer = new StringBuilder();
        
        foreach (var f in files)
        {
            if (!fileDetectedMessage)
            {
                fileDetectedMessage = true;
                Log.Information("Files detected. Start auto-transferring started...");
            }

            Path.GetFileName(f);

            _ = Cli.Wrap("immich")
                .WithArguments($"upload {f}")
                .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
                .ExecuteBufferedAsync()
                .Task
                .Result;

            foreach(var l in stdOutBuffer.ToString().Split(Environment.NewLine))
            {
                if (string.IsNullOrWhiteSpace(l)) continue;
                if (l.Contains("Crawling")) continue;
                if (l.Contains("All")) continue;
                if (l.Contains("1 duplicate"))
                    Log.Information("File {0} already exist. Skipping.", f);
                if (l.Contains("Successfully")) 
                    Log.Information(l.Replace("1", f));
            }
            File.Move(f, f.Replace("pending", "uploaded"), true);
        }

        if (fileDetectedMessage)
        {
            Log.Information("Transfer complete for source {Source}.", source);
        }
    }
    catch (DirectoryNotFoundException)
    {
    }
}

void LoginToImmich(string host, string apiKey)
{
    _ = Cli.Wrap("immich")
        .WithArguments($"login {host}/api {apiKey}")
        .ExecuteBufferedAsync()
        .Task
        .Result;
}

void CreateFolder(string path)
{
    if (!Directory.Exists(path))
        Directory.CreateDirectory(path);
}
