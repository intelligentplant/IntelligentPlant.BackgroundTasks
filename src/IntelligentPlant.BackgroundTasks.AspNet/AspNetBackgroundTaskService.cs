using System;
using System.Threading;
using System.Web.Hosting;

using Microsoft.Extensions.Logging;

namespace IntelligentPlant.BackgroundTasks.AspNet {

    /// <summary>
    /// <see cref="IBackgroundTaskService"/> implementation that uses <see cref="HostingEnvironment.QueueBackgroundWorkItem(Func{CancellationToken, System.Threading.Tasks.Task})"/> 
    /// to register work items with IIS.
    /// </summary>
    public sealed partial class AspNetBackgroundTaskService : BackgroundTaskService {

        /// <summary>
        /// Creates a new <see cref="AspNetBackgroundTaskService"/> object.
        /// </summary>
        /// <param name="options">
        ///   The options for the service.
        /// </param>
        /// <param name="logger">
        ///   The logger for the service.
        /// </param>
        public AspNetBackgroundTaskService(
            BackgroundTaskServiceOptions? options,
            ILogger<AspNetBackgroundTaskService>? logger
        ) : base(options, logger) { }


        /// <inheritdoc/>
        protected override void RunBackgroundWorkItem(BackgroundWorkItem workItem, CancellationToken cancellationToken) {
            HostingEnvironment.QueueBackgroundWorkItem(async ct => {
                using (var ctSource = CancellationTokenSource.CreateLinkedTokenSource(ct, cancellationToken)) {
                    await InvokeWorkItemAsync(workItem, ctSource.Token).ConfigureAwait(false);
                }
            });
        }


        /// <summary>
        /// Runs the background task service in an IIS background task.
        /// </summary>
        /// <remarks>
        ///   Call this method during application startup to run the background task service.
        /// </remarks>
        public void RegisterWithIIS() {
            if (IsRunning) {
                return;
            }

            HostingEnvironment.QueueBackgroundWorkItem(async ct => { 
                try {
                    await RunAsync(ct).ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested) { }
                catch (Exception e) {
                    LogServiceError(Logger, Name, e);
                }
            });
        }


        [LoggerMessage(200, LogLevel.Error, "An error occurred while running background task service '{name}'.")]
        static partial void LogServiceError(ILogger logger, string name, Exception error);

    }
}
