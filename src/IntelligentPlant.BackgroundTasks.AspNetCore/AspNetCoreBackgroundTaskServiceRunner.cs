using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace IntelligentPlant.BackgroundTasks.AspNetCore {

    /// <summary>
    /// <see cref="IHostedService"/> that is used to start and stop the registered 
    /// <see cref="IBackgroundTaskService"/> when the application starts and stops.
    /// </summary>
    public sealed class AspNetCoreBackgroundTaskServiceRunner : IHostedService, IDisposable {

        /// <summary>
        /// The task that dequeues and runs queued background work items.
        /// </summary>
        private Task? _executingTask;

        /// <summary>
        /// Fires when <see cref="StopAsync(CancellationToken)"/> is called or the service is 
        /// disposed.
        /// </summary>
        private readonly CancellationTokenSource _stoppingCts = new CancellationTokenSource();

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


        /// <summary>
        /// Triggered when the application host is ready to start the service.
        /// </summary>
        /// <param name="cancellationToken">
        ///   Indicates that the start process has been aborted.
        /// </param>
        public Task StartAsync(CancellationToken cancellationToken) {
            _executingTask = _backgroundTaskService.RunAsync(_stoppingCts.Token);

            if (_executingTask.IsCompleted) {
                // The task would be completed here if e.g. an error occurred. Return the 
                // underlying task here so that the error etc. becomes visible to the caller.
                return _executingTask;
            }

            return Task.CompletedTask;
        }


        /// <summary>
        /// Triggered when the application host is performing a graceful shutdown.
        /// </summary>
        /// <param name="cancellationToken">
        ///   Indicates that the shutdown process should no longer be graceful.
        /// </param>
        public async Task StopAsync(CancellationToken cancellationToken) {
            // Stop called without start
            if (_executingTask == null) {
                return;
            }

            try {
                // Signal cancellation to the executing method
                _stoppingCts.Cancel();
            }
            finally {
                // Wait until the task completes or the stop token triggers
                await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite, cancellationToken)).ConfigureAwait(false);
            }

        }


        /// <inheritdoc/>
        public void Dispose() {
            _stoppingCts.Dispose();
        }

    }
}
