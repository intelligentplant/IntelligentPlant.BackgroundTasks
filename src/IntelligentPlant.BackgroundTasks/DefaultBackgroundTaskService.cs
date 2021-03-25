using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace IntelligentPlant.BackgroundTasks {

    /// <summary>
    /// Default <see cref="IBackgroundTaskService"/> implementation that runs work items in the 
    /// background using <see cref="Task.Run(Action, CancellationToken)"/> or 
    /// <see cref="Task.Run(Func{Task}, CancellationToken)"/>.
    /// </summary>
    public class DefaultBackgroundTaskService : BackgroundTaskService {

        /// <summary>
        /// A stopwatch for measuring elapsed time for tasks.
        /// </summary>
        private readonly System.Diagnostics.Stopwatch _stopwatch = System.Diagnostics.Stopwatch.StartNew();


        /// <summary>
        /// Creates a new <see cref="DefaultBackgroundTaskService"/> object.
        /// </summary>
        /// <param name="options">
        ///   The options for the service.
        /// </param>
        /// <param name="logger">
        ///   The logger for the service.
        /// </param>
        public DefaultBackgroundTaskService(
            BackgroundTaskServiceOptions? options, 
            ILogger<DefaultBackgroundTaskService>? logger
        ) : base(options, logger) { }


        /// <inheritdoc/>
        protected override void RunBackgroundWorkItem(BackgroundWorkItem workItem, CancellationToken cancellationToken) {
            if (workItem.WorkItem != null) {
                _ = Task.Run(() => {
                    Activity? previousActivity = null;

                    try {
                        if (workItem.ParentActivity != null) {
                            // The work item has captured its parent activity. Store Activity.Current
                            // so that we can restore it later.
                            previousActivity = Activity.Current;
                            Activity.Current = workItem.ParentActivity;
                        }

                        var elapsedBefore = _stopwatch.Elapsed;
                        try {
                            OnRunning(workItem);
                            workItem.WorkItem(cancellationToken);
                            OnCompleted(workItem, _stopwatch.Elapsed - elapsedBefore);
                        }
                        catch (Exception e) {
                            OnError(workItem, e, _stopwatch.Elapsed - elapsedBefore);
                        }
                    }
                    finally {
                        if (workItem.ParentActivity != null) {
                            // Restore the original activity.
                            Activity.Current = previousActivity;
                        }
                    }
                }, cancellationToken);
            }
            else if (workItem.WorkItemAsync != null) {
                _ = Task.Run(async () => {
                    Activity? previousActivity = null;
                    try {
                        if (workItem.ParentActivity != null) {
                            // The work item has captured its parent activity. Store Activity.Current
                            // so that we can restore it later.
                            previousActivity = Activity.Current;
                            Activity.Current = workItem.ParentActivity;
                        }

                        var elapsedBefore = _stopwatch.Elapsed;
                        try {
                            OnRunning(workItem);
                            await workItem.WorkItemAsync(cancellationToken).ConfigureAwait(false);
                            OnCompleted(workItem, _stopwatch.Elapsed - elapsedBefore);
                        }
                        catch (Exception e) {
                            OnError(workItem, e, _stopwatch.Elapsed - elapsedBefore);
                        }
                    }
                    finally {
                        if (workItem.ParentActivity != null) {
                            // Restore the original activity.
                            Activity.Current = previousActivity;
                        }
                    }
                }, cancellationToken);
            }
        }

    }
}
