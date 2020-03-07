using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace IntelligentPlant.BackgroundTasks.AspNetCore {

    /// <summary>
    /// <see cref="IBackgroundTaskService"/> implementation for ASP.NET Core applications.
    /// </summary>
    public sealed class AspNetCoreBackgroundTaskService : DefaultBackgroundTaskService {

        /// <summary>
        /// The logger for the service.
        /// </summary>
        private readonly ILogger _logger;


        /// <summary>
        /// Creates a new <see cref="AspNetCoreBackgroundTaskService"/> object.
        /// </summary>
        /// <param name="options">
        ///   The service options.
        /// </param>
        /// <param name="logger">
        ///   The logger for the service. Errors occurring in background tasks will be written to 
        ///   this logger.
        /// </param>
        public AspNetCoreBackgroundTaskService(BackgroundTaskServiceOptions options, ILogger<AspNetCoreBackgroundTaskService> logger) 
            : base(options) {
            _logger = (ILogger) logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        }


        /// <inheritdoc/>
        protected override void OnQueued(BackgroundWorkItem workItem) {
            if (_logger.IsEnabled(LogLevel.Trace)) {
                _logger.LogTrace(Resources.Log_TaskQueued, workItem);
            }
            base.OnQueued(workItem);
        }


        /// <inheritdoc/>
        protected override void OnDequeued(BackgroundWorkItem workItem) {
            if (_logger.IsEnabled(LogLevel.Trace)) {
                _logger.LogTrace(Resources.Log_TaskDequeued, workItem);
            }
            base.OnDequeued(workItem);
        }


        /// <inheritdoc/>
        protected override void OnRunning(BackgroundWorkItem workItem) {
            if (_logger.IsEnabled(LogLevel.Trace)) {
                _logger.LogTrace(Resources.Log_TaskRunning, workItem);
            }
            base.OnRunning(workItem);
        }


        /// <inheritdoc/>
        protected override void OnCompleted(BackgroundWorkItem workItem) {
            if (_logger.IsEnabled(LogLevel.Trace)) {
                _logger.LogTrace(Resources.Log_TaskCompleted, workItem);
            }
            base.OnCompleted(workItem);
        }


        /// <inheritdoc/>
        protected override void OnError(BackgroundWorkItem workItem, Exception error) {
            // Don't need to log cancellations.
            if (!(error is OperationCanceledException)) {
                _logger.LogError(error, Resources.Log_TaskError, workItem);
            }
            base.OnError(workItem, error);
        }

    }
}
