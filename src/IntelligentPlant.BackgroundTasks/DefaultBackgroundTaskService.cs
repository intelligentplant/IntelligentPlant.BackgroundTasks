using System;
using System.Threading;
using System.Threading.Tasks;

namespace IntelligentPlant.BackgroundTasks {

    /// <summary>
    /// Default <see cref="IBackgroundTaskService"/> implementation that runs work items in the 
    /// background using <see cref="Task.Run(Action, CancellationToken)"/> or 
    /// <see cref="Task.Run(Func{Task}, CancellationToken)"/>.
    /// </summary>
    public class DefaultBackgroundTaskService : BackgroundTaskService {

        /// <summary>
        /// Creates a new <see cref="DefaultBackgroundTaskService"/> object.
        /// </summary>
        /// <param name="options">
        ///   The options for the service.
        /// </param>
        public DefaultBackgroundTaskService(BackgroundTaskServiceOptions options) 
            : base(options) { }


        /// <inheritdoc/>
        protected override void RunBackgroundWorkItem(BackgroundWorkItem workItem, CancellationToken cancellationToken) {
            if (workItem.WorkItem != null) {
                _ = Task.Run(() => {
                    try {
                        OnRunning(workItem);
                        workItem.WorkItem(cancellationToken);
                        OnCompleted(workItem);
                    }
                    catch (Exception e) {
                        OnError(workItem, e);
                    }
                }, cancellationToken);
            }
            else if (workItem.WorkItemAsync != null) {
                _ = Task.Run(async () => {
                    try {
                        OnRunning(workItem);
                        await workItem.WorkItemAsync(cancellationToken).ConfigureAwait(false);
                        OnCompleted(workItem);
                    }
                    catch (Exception e) {
                        OnError(workItem, e);
                    }
                }, cancellationToken);
            }
        }

    }
}
