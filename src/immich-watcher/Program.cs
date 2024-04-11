using System.Text;
using CliWrap;
using CliWrap.Buffered;
using Serilog;


var sourcePath = "/var/lib/data";

var extensionFiles = new[] { ".jpg", ".mp4" };

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

if (!Directory.Exists(sourcePath))
{
    Log.Information("IMMICH_WATCHER_PATH with value {0} doesn't exit. Terminating", sourcePath);
    return;
}

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

CreateFolder(Path.Combine(sourcePath, "pending"));
CreateFolder(Path.Combine(sourcePath, "uploaded"));

Log.Information("Started monitoring source: {Source} folder.", sourcePath);

try
{

}
catch (Exception e)
{
    Console.WriteLine(e);
    throw;
}

var loginSuccess = LoginToImmich();

while (true)
{
    if (loginSuccess)
    {
        StartTransferringFiles(sourcePath);
    }
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

            OutputMessage(stdOutBuffer, f);
            
            File.Move(f, f.Replace("pending", "uploaded"), true);
        }
    }
    catch (Exception e)
    {
        Log.Error(e, e.Message);
    }
}

bool LoginToImmich()
{
    try
    {
        var host =  Environment.GetEnvironmentVariable("IMMICH_HOST");
        var apiKey = Environment.GetEnvironmentVariable("IMMICH_API_KEY");
        
        if (string.IsNullOrWhiteSpace(host))
        {
            Log.Information("IMMICH_HOST is missing. Please fixed the problem and restart the container.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            Log.Information("IMMICH_API_KEY is missing. Please fixed the problem and restart the container.");
            return false;
        }
        
        _ = Cli.Wrap("immich")
            .WithArguments($"login {host}/api {apiKey}")
            .ExecuteBufferedAsync()
            .Task
            .Result;
        
        Log.Information("Login to Immich Server: {0} successful.", host);

        return true; 
    }
    catch (Exception e)
    {
        Log.Error(e, e.Message);
    }
    return false; 
}

void CreateFolder(string path)
{
    if (!Directory.Exists(path))
        Directory.CreateDirectory(path);
}

void OutputMessage(StringBuilder stringBuilder, string s)
{
    foreach (var l in stringBuilder.ToString().Split(Environment.NewLine))
    {
        if (l.Contains("1 duplicate"))
            Log.Information("File {0} already exist. Skipping.", s);
        if (l.Contains("Successfully"))
            Log.Information(l.Replace("1", s));
    }
}
