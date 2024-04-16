namespace Siganberg.ImmichWatcher.Tests.Integration.Helpers;

public class TestServer
{
    private const string ImmichUrl = "http://localhost:3001";
    public readonly ImmichApiManager ImmichApiManager;

    public TestServer()
    {
        DockerHelper.TryStartInfrastructureDependencies();

        ImmichApiManager = new ImmichApiManager(ImmichUrl, "test@test.com", "test");
        ImmichApiManager.InitializedServerAsync().GetAwaiter().GetResult();
        
        SetupConfigOverridesThroughEnvironmentVariable();
        StartWatcher();
    }

    private static void StartWatcher()
    {
        var cancellationTokenSource = new CancellationTokenSource();
        _ = Program.MainAsync(cancellationTokenSource.Token);
    }

    private void SetupConfigOverridesThroughEnvironmentVariable()
    {
        var workingDirectory = Environment.CurrentDirectory;
        var uploadPath = Path.Combine(Directory.GetParent(workingDirectory)!.Parent!.Parent!.FullName) + "/TestData";
        Environment.SetEnvironmentVariable("IMMICH_UPLOAD_PATH",  uploadPath);
        Environment.SetEnvironmentVariable("IMMICH_INSTANCE_URL", ImmichUrl);
        Environment.SetEnvironmentVariable("IMMICH_API_KEY",  ImmichApiManager.ApiKey.Secret);
        
    }

}