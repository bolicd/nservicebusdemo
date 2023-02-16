using System.Diagnostics;
using NServiceBus;

namespace WorkerService1;

public class CriticalAction
{
    public static async Task OnCriticalError(ICriticalErrorContext context, CancellationToken cancellationToken)
    {
        var fatalMessage =
            $"The following critical error was encountered:{Environment.NewLine}{context.Error}{Environment.NewLine}Process is shutting down. StackTrace: {Environment.NewLine}{context.Exception.StackTrace}";

        EventLog.WriteEntry(".NET Runtime", fatalMessage, EventLogEntryType.Error);

        try
        {
            await context.Stop(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            Environment.FailFast(fatalMessage, context.Exception);
        }
    }
}