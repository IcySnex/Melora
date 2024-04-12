using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Display;

namespace Musify.Helpers;

public class InMemorySink : ILogEventSink
{
    readonly MessageTemplateTextFormatter textFormatter = new("[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}");

    public event EventHandler<string>? OnNewLog;

    public void Emit(
        LogEvent logEvent)
    {
        StringWriter renderSpace = new();
        textFormatter.Format(logEvent, renderSpace);

        OnNewLog?.Invoke(this, renderSpace.ToString());
    }
}