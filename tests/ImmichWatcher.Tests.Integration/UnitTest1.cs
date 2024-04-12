using FluentAssertions;
using FluentAssertions.Extensions;

namespace Siganberg.ImmichWatcher.Tests.Integration;

public class ProgramTests
{
    [Fact]
    public async Task GivenMissingImmichHostEnv_WhenSomethingStart_ThenShouldSeeErrorMissingImmichHost()
    {
        // Arrange
        Environment.SetEnvironmentVariable("IMMICH_HOST", "");
        Environment.SetEnvironmentVariable("IMMICH_API_KEY", "somevalue");
        var outputWriter = new StringWriter();
        Console.SetOut(outputWriter);
        var cts = new CancellationTokenSource();

        // Act
        _ = Program.MainAsync(cts.Token);

        // Assert
        var asyncTest = () =>   outputWriter.ToString().Should().Contain("ERR] IMMICH_HOST is missing");
        asyncTest.Should().NotThrowAfter(2.Seconds(), 100.Milliseconds());
        await cts.CancelAsync();
    }
    
    [Fact]
    public async Task GivenMissingAouJet_WhenSomethingStart_ThenShouldSeeErrorApiKey()
    {
        // Arrange
        Environment.SetEnvironmentVariable("IMMICH_HOST", "somevalue");
        Environment.SetEnvironmentVariable("IMMICH_API_KEY", "");
        var outputWriter = new StringWriter();
        Console.SetOut(outputWriter);
        var cts = new CancellationTokenSource();

        // Act
        _ = Program.MainAsync(cts.Token);

        // Assert
        var asyncTest = () =>   outputWriter.ToString().Should().Contain("ERR] IMMICH_API_KEY is missing");
        asyncTest.Should().NotThrowAfter(2.Seconds(), 100.Milliseconds());
        await cts.CancelAsync();
    }
}


