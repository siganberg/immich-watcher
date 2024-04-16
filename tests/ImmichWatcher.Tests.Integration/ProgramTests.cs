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
}