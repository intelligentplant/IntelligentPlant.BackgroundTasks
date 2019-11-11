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
        protected override void OnError(Exception error, Action<CancellationToken> workItem) {
            _logger.LogError(error, Resources.Log_ErrorInBackgroundTask, workItem);
            base.OnError(error, workItem);
        }


        /// <inheritdoc/>
        protected override void OnError(Exception error, Func<CancellationToken, Task> workItem) {
            _logger.LogError(error, Resources.Log_ErrorInBackgroundTask, workItem);
            base.OnError(error, workItem);
        }

    }
}
