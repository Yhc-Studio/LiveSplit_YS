using System;
using System.Diagnostics;
using System.Threading;

namespace LiveSplit.Options;

/// <summary>
/// LiveSplit YS logging facade.
///
/// Stage 4.7 deliberately avoids touching Windows Event Log on the first
/// informational log message. Some components (notably Scriptable Auto Splitter)
/// call Log.Info while a layout is becoming interactive. EventLog.SourceExists can
/// take seconds on some Windows installations and used to block the UI thread.
///
/// Warning/error logging starts Event Log registration asynchronously. The current
/// message is still sent to the already configured Trace listeners immediately;
/// later warnings/errors are also picked up by the EventLog listener once ready.
/// </summary>
public static class Log
{
    private static int eventLogInitializationStarted;

    private static void StartEventLogInitialization()
    {
        if (Interlocked.CompareExchange(ref eventLogInitializationStarted, 1, 0) != 0)
        {
            return;
        }

        ThreadPool.QueueUserWorkItem(_ =>
        {
            try
            {
                if (!EventLog.SourceExists("LiveSplit"))
                {
                    EventLog.CreateEventSource("LiveSplit", "Application");
                }

                var listener = new EventLogTraceListener("LiveSplit")
                {
                    Filter = new EventTypeFilter(SourceLevels.Warning)
                };
                Trace.Listeners.Add(listener);
            }
            catch
            {
                // Logging must never interfere with the timer UI.
            }
        });
    }

    public static void Error(Exception ex)
    {
        StartEventLogInitialization();
        try
        {
            Trace.TraceError("{0}\n\n{1}", ex.Message, ex.StackTrace);
        }
        catch { }
    }

    public static void Error(string message)
    {
        StartEventLogInitialization();
        try
        {
            Trace.TraceError(message);
        }
        catch { }
    }

    public static void Info(string message)
    {
        // Important: do not initialize Windows Event Log here. ASL loads use
        // Log.Info on the UI thread, and EventLog.SourceExists is comparatively
        // expensive. The default/debug Trace listeners are sufficient for info.
        try
        {
            Trace.TraceInformation(message);
        }
        catch { }
    }

    public static void Warning(string message)
    {
        StartEventLogInitialization();
        try
        {
            Trace.TraceWarning(message);
        }
        catch { }
    }
}
