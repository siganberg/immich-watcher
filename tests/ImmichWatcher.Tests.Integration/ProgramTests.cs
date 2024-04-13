using FluentAssertions;
using FluentAssertions.Extensions;
using Siganberg.ImmichWatcher.Tests.Integration.Helpers;

namespace Siganberg.ImmichWatcher.Tests.Integration;

public class ProgramTests
{
    [Fact]
    public async Task GivenMissingImmichHostEnv_WhenSomethingStart_ThenShouldSeeErrorMissingImmichHost()
    {
        // Arrange
        Environment.SetEnvironmentVariable("IMMICH_HOST", "");
        Environment.SetEnvironmentVariable("IMMICH_API_KEY", "somevalue");
        using var consoleOutput = new ConsoleOutput();
        
        // Act
        var cts = new CancellationTokenSource();
        _ = Program.MainAsync(cts.Token);

        // Assert
        var asyncTest = () => consoleOutput.GetOutput().Should().Contain("ERR] IMMICH_HOST is missing");
        asyncTest.Should().NotThrowAfter(2.Seconds(), 100.Milliseconds());
        await cts.CancelAsync();
        
    }
    
    [Fact]
    public async Task GivenMissingAouJet_WhenSomethingStart_ThenShouldSeeErrorApiKey()
    {
        // Arrange
        Environment.SetEnvironmentVariable("IMMICH_HOST", "somevalue");
        Environment.SetEnvironmentVariable("IMMICH_API_KEY", "");
        using var consoleOutput = new ConsoleOutput();

        // Act
        var cts = new CancellationTokenSource();
        _ = Program.MainAsync(cts.Token);

        // Assert
        var asyncTest = () => consoleOutput.GetOutput().Should().Contain("ERR] IMMICH_API_KEY is missing");
        asyncTest.Should().NotThrowAfter(2.Seconds(), 100.Milliseconds());
        await cts.CancelAsync();
    }
}


