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
                    var previousActivity = Activity.Current;
                    try {
                        Activity.Current = workItem.ParentActivity;
                        using (var activity = workItem.StartActivity()) {
                            var elapsedBefore = _stopwatch.Elapsed;
                            try {
                                OnRunning(workItem);
                                workItem.WorkItem(cancellationToken);
                                OnCompleted(workItem, _stopwatch.Elapsed - elapsedBefore);
                            }
                            catch (Exception e) {
                                OnError(workItem, e, _stopwatch.Elapsed - elapsedBefore);
                                // Add an OpenTelemetry tag warning that the item faulted.
                                // https://github.com/open-telemetry/opentelemetry-dotnet/blob/main/src/OpenTelemetry.Api/README.md#setting-status
                                activity?.SetTag("otel.status_description", e.Message);
                            }
                        }
                    }
                    finally {
                        Activity.Current = previousActivity;
                    }
                }, cancellationToken);
            }
            else if (workItem.WorkItemAsync != null) {
                _ = Task.Run(async () => {
                    var previousActivity = Activity.Current;
                    try {
                        Activity.Current = workItem.ParentActivity;
                        using (var activity = workItem.StartActivity()) {
                            var elapsedBefore = _stopwatch.Elapsed;
                            try {
                                OnRunning(workItem);
                                await workItem.WorkItemAsync(cancellationToken).ConfigureAwait(false);
                                OnCompleted(workItem, _stopwatch.Elapsed - elapsedBefore);
                            }
                            catch (Exception e) {
                                OnError(workItem, e, _stopwatch.Elapsed - elapsedBefore);
                                // Add an OpenTelemetry tag warning that the item faulted.
                                // https://github.com/open-telemetry/opentelemetry-dotnet/blob/main/src/OpenTelemetry.Api/README.md#setting-status
                                activity?.SetTag("otel.status_description", e.Message);
                            }
                        }
                    }
                    finally {
                        Activity.Current = previousActivity;
                    }
                }, cancellationToken);
            }
        }

    }
}
