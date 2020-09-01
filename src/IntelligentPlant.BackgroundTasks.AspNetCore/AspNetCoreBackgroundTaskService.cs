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
        /// Creates a new <see cref="AspNetCoreBackgroundTaskService"/> object.
        /// </summary>
        /// <param name="options">
        ///   The service options.
        /// </param>
        /// <param name="logger">
        ///   The logger for the service. Errors occurring in background tasks will be written to 
        ///   this logger.
        /// </param>
        public AspNetCoreBackgroundTaskService(
            BackgroundTaskServiceOptions? options, 
            ILogger<AspNetCoreBackgroundTaskService>? logger
        ) : base(options, logger) { }

    }
}
