using System.Diagnostics.CodeAnalysis;
using System.Text;
using CliWrap;
using CliWrap.Buffered;
using Microsoft.Extensions.Logging;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;


namespace Siganberg.ImmichWatcher;

public class Program
{
    private const string SourcePath = "/var/lib/data";
    private const string PendingName = "pending";
    private const string UploadedName = "uploaded";

    private static readonly ILogger Logger = new LoggerFactory()
        .AddSerilog(new LoggerConfiguration().WriteTo.Console(outputTemplate:"[{Timestamp:MM/dd/yyy HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}").CreateLogger())
        .CreateLogger<Program>();
    
    [ExcludeFromCodeCoverage]
    public static void Main()
    {
        var cancellationTokenSource =  new CancellationTokenSource();
        MainAsync(cancellationTokenSource.Token).GetAwaiter().GetResult();
    }
    public static async Task MainAsync(CancellationToken cancellationToken)
    {
        CreateFolder(Path.Combine(SourcePath, PendingName));
        CreateFolder(Path.Combine(SourcePath, PendingName));

        Logger.LogInformation("Started monitoring source: {Source} folder.", SourcePath);

        var loginSuccess = await LoginToImmichAsync(cancellationToken);

        while (!cancellationToken.IsCancellationRequested)
        {
            if (loginSuccess)
            {
                await StartTransferringFilesAsync(SourcePath, cancellationToken);
            }
            await Task.Delay(1000, cancellationToken);
        }
    }
    static async Task StartTransferringFilesAsync(string source, CancellationToken cancellationToken)
    {
        var extensionFiles =  new[] { ".jpg", ".mp4" };
        try
        {
            var files = Directory.EnumerateFiles(Path.Combine(source, PendingName), "*.*", SearchOption.AllDirectories)
                .Where(a =>
                {
                    var lower = Path.GetFileName(a).ToLower();
                    return extensionFiles.Any(b => b == Path.GetExtension(lower));
                });

            var outBuffer = new StringBuilder();

            foreach (var file in files)
            {
                _ = await Cli.Wrap("immich")
                    .WithArguments($"upload {file}")
                    .WithStandardOutputPipe(PipeTarget.ToStringBuilder(outBuffer))
                    .ExecuteBufferedAsync(cancellationToken);

                MapOutputMessage(outBuffer, file);

                File.Move(file, file.Replace(PendingName, UploadedName), true);
            }
        }
        catch (Exception e)
        {
            Logger.LogError(e, e.Message);
        }
    }

    static async Task<bool> LoginToImmichAsync(CancellationToken cancellationToken)
    {
        try
        {
            var host = Environment.GetEnvironmentVariable("IMMICH_HOST");
            var apiKey = Environment.GetEnvironmentVariable("IMMICH_API_KEY");

            if (string.IsNullOrWhiteSpace(host))
            {
                Logger.LogError("IMMICH_HOST is missing. Please fixed the problem and restart the container.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                Logger.LogError("IMMICH_API_KEY is missing. Please fixed the problem and restart the container.");
                return false;
            }

            _ = await Cli.Wrap("immich")
                .WithArguments($"login {host}/api {apiKey}")
                .ExecuteBufferedAsync(cancellationToken);
         

            Logger.LogInformation("Login to Immich Server: {0} successful.", host);

            return true;
        }
        catch (Exception e)
        {
            Logger.LogInformation(e, "Error trying to login to Immich Server. Please correct the error and restart the container.");
        }

        return false;
    }

    static void CreateFolder(string path)
    {
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
    }

    static void MapOutputMessage(StringBuilder stringBuilder, string s)
    {
        foreach (var l in stringBuilder.ToString().Split(Environment.NewLine))
        {
            if (l.Contains("1 duplicate"))
                Logger.LogInformation("File {0} already exist. Skipping.", s);
            if (l.Contains("Successfully"))
                Logger.LogInformation(l.Replace("1", s));
        }
    }
}