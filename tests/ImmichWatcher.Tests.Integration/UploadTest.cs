using Siganberg.ImmichWatcher.Tests.Integration.Helpers;

namespace Siganberg.ImmichWatcher.Tests.Integration;

[Collection(nameof(TestServerCollection))]
public class UploadTest
{
    [Fact]
    public void GivenMissingApiKey_WhenRun_ThenShouldSeeErrorApiKey()
    {
        // Arrange
        using var consoleOutput = new ConsoleOutput();

        // Act
      
        var testDataPath = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory())!.Parent!.Parent!.FullName, "TestData");
        var pendingPath = Path.Combine(testDataPath, "pending");
        var di = new DirectoryInfo(Path.Combine(pendingPath));
        foreach (var file in di.GetFiles())
            file.Delete(); 
        File.Copy(Path.Combine(testDataPath, "RiskyRiders.jpg"), Path.Combine(pendingPath, "RiskyRiders.jpg"));
        
        // Assert
        Thread.Sleep(20000);
  
    }
}