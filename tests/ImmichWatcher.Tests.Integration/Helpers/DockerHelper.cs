using CliWrap;
using CliWrap.Buffered;

namespace Siganberg.ImmichWatcher.Tests.Integration.Helpers;

public static class DockerHelper
{
    public static void TryStartInfrastructureDependencies()
    {
        var targetPath = Directory.GetCurrentDirectory();
        targetPath = Path.Combine(Directory.GetParent(targetPath)!.Parent!.Parent!.FullName, "Compose");

        var result =  Cli.Wrap("docker")
            .WithArguments("compose up -d")
            .WithWorkingDirectory(targetPath)
            .ExecuteBufferedAsync()
            .Task
            .Result;

        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("Executing docker-compose for infrastructure.");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(result.StandardError);
    }
}