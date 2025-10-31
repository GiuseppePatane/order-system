using Microsoft.Extensions.Logging;

namespace ApiGateway.infrastructure.GrcpClient.Extensions;

public static class TaskExtensions
{
    /// <summary>
    /// Observes the task to avoid the UnobservedTaskException event to be raised.
    /// </summary>
    public static void Forget(this Task task, ILogger logger)
    {
        logger.LogDebug("Start Forget");
        if (!task.IsCompleted || task.IsFaulted)
        {
            logger.LogDebug("executing ForgetAwaited");
            _ = ForgetAwaited(task, logger);
            logger.LogDebug("executed ForgetAwaited");
        }
        logger.LogDebug("End Forget");
        return;
        
        static async Task ForgetAwaited(Task task, ILogger logger)
        {
            try
            {
                logger.LogDebug("executing await task");
                await task.ConfigureAwait(false);
                logger.LogDebug("executed await task");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
            }
        }
    }
}