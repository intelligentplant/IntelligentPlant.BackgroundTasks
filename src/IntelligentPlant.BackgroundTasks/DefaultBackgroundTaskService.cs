using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace IntelligentPlant.BackgroundTasks {

    /// <summary>
    /// Default <see cref="IBackgroundTaskService"/> implementation that runs work items in the 
    /// background using <see cref="Task.Run(Action, CancellationToken)"/> or 
    /// <see cref="Task.Run(Func{Task}, CancellationToken)"/>.
    /// </summary>
    public sealed class DefaultBackgroundTaskService : BackgroundTaskService {

        /// <summary>
        /// Creates a new <see cref="DefaultBackgroundTaskService"/> object.
        /// </summary>
        /// <param name="logger">
        ///   The logger for the service.
        /// </param>
        public DefaultBackgroundTaskService(ILogger<DefaultBackgroundTaskService> logger) 
            : base(logger) { }


        /// <inheritdoc/>
        protected override void RunBackgroundWorkItem(Action<CancellationToken> workItem, CancellationToken cancellationToken) {
            _ = Task.Run(() => {
                try {
                    workItem(cancellationToken);
                }
                catch (OperationCanceledException) {
                    // Do nothing
                }
                catch (Exception e) {
                    Logger.LogError(e, Resources.Log_ErrorInBackgroundTask, workItem);
                }
            }, cancellationToken);
        }


        /// <inheritdoc/>
        protected override void RunBackgroundWorkItem(Func<CancellationToken, Task> workItem, CancellationToken cancellationToken) {
            _ = Task.Run(async () => {
                try {
                    await workItem(cancellationToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException) {
                    // Do nothing
                }
                catch (Exception e) {
                    Logger.LogError(e, Resources.Log_ErrorInBackgroundTask, workItem);
                }
            }, cancellationToken);
        }

    }
}
