using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;


namespace Siganberg.ImmichWatcher;

public static class Program
{
    private const string DefaultSourcePath = "/var/lib/data";
    private const string PendingName = "pending";
    private const string UploadedName = "uploaded";

    private static ImmichApiManager _immichApiManager = default!;

    private static readonly ILogger Logger = new LoggerFactory()
        .AddSerilog(new LoggerConfiguration().WriteTo
            .Console(outputTemplate: "[{Timestamp:MM/dd/yyy HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger())
        .CreateLogger("Program");


    [ExcludeFromCodeCoverage]
    public static void Main()
    {
        var cancellationTokenSource = new CancellationTokenSource();
        MainAsync(cancellationTokenSource.Token).GetAwaiter().GetResult();
    }

    public static async Task MainAsync(CancellationToken cancellationToken)
    {
        var apiKey = Environment.GetEnvironmentVariable("IMMICH_API_KEY");
        if (string.IsNullOrEmpty(apiKey))
        {
            Logger.LogError("IMMICH_API_KEY environment variable is missing.");
            await Task.Delay(30000, cancellationToken);
            return;
        }
        
        _immichApiManager = new ImmichApiManager(apiKey, Environment.GetEnvironmentVariable("IMMICH_INSTANCE_URL"));

        var sourcePath = Environment.GetEnvironmentVariable("IMMICH_UPLOAD_PATH");
        if (string.IsNullOrWhiteSpace(sourcePath) || !Directory.Exists(sourcePath))
            sourcePath = DefaultSourcePath;


        CreateFolder(Path.Combine(sourcePath, PendingName));
        CreateFolder(Path.Combine(sourcePath, UploadedName));

        Logger.LogInformation("Started monitoring source: {Source} folder.", sourcePath);

        var supportedMedia = await _immichApiManager.GetSupportedMediaTypes();
        var extensionFiles = supportedMedia.Image.Union(supportedMedia.Video)
            .GroupBy(a => a)
            .Select(a => a.Key)
            .ToHashSet();

        
        while (!cancellationToken.IsCancellationRequested)
        {
            await StartTransferringFilesAsync(sourcePath, extensionFiles, cancellationToken);
            await Task.Delay(1000, cancellationToken);
        }
    }

    static async Task StartTransferringFilesAsync(string source, IEnumerable<string> extensionFiles,  CancellationToken cancellationToken)
    {
      
        try
        {
            var files = Directory.EnumerateFiles(Path.Combine(source, PendingName), "*.*", SearchOption.AllDirectories)
                .Where(a =>
                {
                    var lower = Path.GetFileName(a).ToLower();
                    return extensionFiles.Any(b => b == Path.GetExtension(lower));
                });
            foreach (var file in files)
            {
                if (await UploadFileAsync(cancellationToken, file))
                {
                    var destination = file.Replace(PendingName, UploadedName);
                    File.Move(file,destination , true);
                }
                 
            }
        }
        catch (Exception e)
        {
            Logger.LogError(e, e.Message);
        }
    }

    private static async Task<bool> UploadFileAsync(CancellationToken cancellationToken, string file)
    {
        try
        {
            var uploadResponse = await _immichApiManager.UploadFileAsync(file, cancellationToken);
            Logger.LogInformation(uploadResponse.Duplicate
                ? "File {file} already exist as assetId: {assetId}."
                : "File {file} uploaded successfully with assetId: {assetId} .", file, uploadResponse.Id);
            return true; 
        }
        catch (Exception e)
        {
            Logger.LogError(e, e.Message);
        }

        return false;
    }

    static void CreateFolder(string path)
    {
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
    }
}