using FluentAssertions;
using FluentAssertions.Extensions;
using Siganberg.ImmichWatcher.Tests.Integration.Helpers;

namespace Siganberg.ImmichWatcher.Tests.Integration;

public class ProgramTests
{
    [Fact]
    public async Task GivenMissingApiKey_WhenRun_ThenShouldSeeErrorApiKey()
    {
        // Arrange
        Environment.SetEnvironmentVariable("IMMICH_INSTANCE_URL", "somevalue");
        Environment.SetEnvironmentVariable("IMMICH_API_KEY", "");
        using var consoleOutput = new ConsoleOutput();

        // Act
        var cts = new CancellationTokenSource();
        _ = Task.Run(() => Program.MainAsync(cts.Token), cts.Token);

        // Assert
        var asyncTest = () => consoleOutput.GetOutput().Should().Contain("ERR] IMMICH_API_KEY environment variable is missing.");
        asyncTest.Should().NotThrowAfter(5.Seconds(), 100.Milliseconds());
        await cts.CancelAsync();
    }
    //
    // [Fact]
    // public async Task GivenJpegExist_WhenRun_ThenFileShouldBeUploaded()
    // {
    //     // Arrange
    //     var workingDirectory = Environment.CurrentDirectory;
    //     var uploadPath = Path.Combine(Directory.GetParent(workingDirectory)!.Parent!.Parent!.FullName) + "/TestData";
    //     Environment.SetEnvironmentVariable("IMMICH_UPLOAD_PATH", uploadPath);
    //     Environment.SetEnvironmentVariable("IMMICH_INSTANCE_URL", "http://192.168.68.20:2283");
    //     Environment.SetEnvironmentVariable("IMMICH_API_KEY", "b3VryThvjEbD9vvAKPvPUPYZ4SiszC3Of1RAgABC5kI");
    //     using var consoleOutput = new ConsoleOutput();
    //
    //     // Act
    //     var cancellationTokenSource = new CancellationTokenSource();
    //     _ = Program.MainAsync(cancellationTokenSource.Token);
    //
    //     // Assert
    //     var asyncTest = () => consoleOutput.GetOutput().Should().Contain("ERR] IMMICH_API_KEY is missing");
    //     asyncTest.Should().NotThrowAfter(200.Seconds(), 100.Milliseconds());
    //     await cancellationTokenSource.CancelAsync();
    // }
}