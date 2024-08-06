using Serilog.Core;
using Serilog.Events;

namespace Musify.Helpers;

public class InMemorySink : ILogEventSink
{
    public event EventHandler<LogEvent>? OnNewLog;

    public void Emit(
        LogEvent logEvent) =>
        OnNewLog?.Invoke(this, logEvent);
}