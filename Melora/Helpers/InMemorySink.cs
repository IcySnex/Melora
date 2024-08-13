using Serilog.Core;
using Serilog.Events;

namespace Melora.Helpers;

public class InMemorySink : ILogEventSink
{
    public event EventHandler<LogEvent>? OnNewLog;

    public void Emit(
        LogEvent logEvent) =>
        OnNewLog?.Invoke(this, logEvent);
}