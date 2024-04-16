using System.ComponentModel;

namespace Siganberg.ImmichWatcher.Tests.Integration.Helpers;

public class ConsoleOutput : IDisposable
{
    private readonly StringWriterExt _stringWriter;
    private readonly TextWriter _originalOutput;

    public ConsoleOutput()
    {
        _originalOutput = Console.Out;
        _stringWriter = new StringWriterExt(true);
        _stringWriter.Flushed += (sender, _) =>
        {
            _originalOutput.Write(sender.ToString());
        };
        Console.SetOut(_stringWriter);
    }
    public string GetOutput()
    {
        var result = _stringWriter.ToString();
        return result;
    }

    public void Dispose()
    {
        Console.SetOut(_originalOutput);
        _stringWriter.Dispose();
    }
}

public sealed class StringWriterExt : StringWriter
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public delegate void FlushedEventHandler(object sender, EventArgs args);
    public event FlushedEventHandler? Flushed;
    private bool AutoFlush { get; set; }

    public StringWriterExt(bool autoFlush)
    {
        AutoFlush = autoFlush;
    }
    private void OnFlush()
    {
        var eh = Flushed;
        if (eh != null)
            eh(this, EventArgs.Empty);
    }

    public override void Flush()
    {
        base.Flush();
        OnFlush();
    }

}