using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;

namespace IntelligentPlant.BackgroundTasks.AspNetCore {

    /// <summary>
    /// <see cref="IHostedService"/> that is used to start and stop the registered 
    /// <see cref="IBackgroundTaskService"/> when the application starts and stops.
    /// </summary>
    public sealed class AspNetCoreBackgroundTaskServiceRunner : BackgroundService {

        /// <summary>
        /// The background task service.
        /// </summary>
        private readonly BackgroundTaskService _backgroundTaskService;


        /// <summary>
        /// Creates a new <see cref="AspNetCoreBackgroundTaskServiceRunner"/>.
        /// </summary>
        /// <param name="backgroundTaskService">
        ///   The <see cref="AspNetCoreBackgroundTaskService"/> that will dequeue and run 
        ///   background tasks.
        /// </param>
        public AspNetCoreBackgroundTaskServiceRunner(IBackgroundTaskService backgroundTaskService) {
            _backgroundTaskService = (BackgroundTaskService) backgroundTaskService;
            // Replace default background task service with the one we have been provided.
            BackgroundTaskService.Default = _backgroundTaskService;
        }


        /// <inheritdoc/>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            await _backgroundTaskService.RunAsync(stoppingToken).ConfigureAwait(false);
        }

    }
}
