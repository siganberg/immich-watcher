namespace Siganberg.ImmichWatcher.Tests.Integration.Helpers;

public class ConsoleOutput : IDisposable
{
    private readonly StringWriter _stringWriter;
    private readonly TextWriter _originalOutput;

    public ConsoleOutput()
    {
        _stringWriter = new StringWriter();
        _originalOutput = Console.Out;
        Console.SetOut(_stringWriter);
    }
    public string GetOutput()
    {
        var result = _stringWriter.ToString();
        _originalOutput.Write(result);
        return result;
    }

    public void Dispose()
    {
        Console.SetOut(_originalOutput);
        _stringWriter.Dispose();
    }
}