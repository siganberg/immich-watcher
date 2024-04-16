using FluentAssertions;
using FluentAssertions.Extensions;
using Siganberg.ImmichWatcher.Tests.Integration.Helpers;

namespace Siganberg.ImmichWatcher.Tests.Integration;

[Collection(nameof(TestServerCollection))]
public class UploadTest
{
    [Fact]
    public async Task GivenPendingFileExist_WhenRun_ThenShouldSeeErrorApiKey()
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
        var asyncTest = () => consoleOutput.GetOutput().Should().Contain("uploaded successfully");
        asyncTest.Should().NotThrowAfter(5.Seconds(), 100.Milliseconds());
    }
}