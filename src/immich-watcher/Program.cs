using System.Diagnostics.CodeAnalysis;
using System.Text;
using CliWrap;
using CliWrap.Buffered;
//using Serilog;
using Microsoft.Extensions.Logging;

namespace Siganberg.ImmichWatcher;

public class Program
{
    const string SourcePath = "/var/lib/data";
    const string PendingName = "pending";
    const string UploadedName = "uploaded";


    private static ILogger _logger = LoggerFactory.Create(builder => { builder.AddConsole();} ).CreateLogger(nameof(Program));
    
    [ExcludeFromCodeCoverage]
    public static void Main()
    {
        var cts =  new CancellationTokenSource();
        MainAsync(cts.Token).GetAwaiter().GetResult();
    }
    public static async Task MainAsync(CancellationToken cancellationToken)
    {
        // Log.Logger = new LoggerConfiguration()
        //     .WriteTo.Console(outputTemplate:"[{Timestamp:MM/dd/yyy HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
        //     .CreateLogger();

        CreateFolder(Path.Combine(SourcePath, PendingName));
        CreateFolder(Path.Combine(SourcePath, PendingName));

        _logger.LogInformation("Started monitoring source: {Source} folder.", SourcePath);

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

                OutputMessage(outBuffer, file);

                File.Move(file, file.Replace(PendingName, UploadedName), true);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
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
                _logger.LogError("IMMICH_HOST is missing. Please fixed the problem and restart the container.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                _logger.LogError("IMMICH_API_KEY is missing. Please fixed the problem and restart the container.");
                return false;
            }

            _ = await Cli.Wrap("immich")
                .WithArguments($"login {host}/api {apiKey}")
                .ExecuteBufferedAsync(cancellationToken);
         

            _logger.LogInformation("Login to Immich Server: {0} successful.", host);

            return true;
        }
        catch (Exception e)
        {
            _logger.LogInformation(e, "Error trying to login to Immich Server. Please correct the error and restart the container.");
        }

        return false;
    }

    static void CreateFolder(string path)
    {
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
    }

    static void OutputMessage(StringBuilder stringBuilder, string s)
    {
        foreach (var l in stringBuilder.ToString().Split(Environment.NewLine))
        {
            if (l.Contains("1 duplicate"))
                _logger.LogInformation("File {0} already exist. Skipping.", s);
            if (l.Contains("Successfully"))
                _logger.LogInformation(l.Replace("1", s));
        }
    }
}