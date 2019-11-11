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
        protected override void RunBackgroundWorkItem(Action<CancellationToken> workItem, CancellationToken cancellationToken) {
            _ = Task.Run(() => {
                try {
                    workItem(cancellationToken);
                }
                catch (Exception e) {
                    OnError(e, workItem);
                }
            }, cancellationToken);
        }


        /// <inheritdoc/>
        protected override void RunBackgroundWorkItem(Func<CancellationToken, Task> workItem, CancellationToken cancellationToken) {
            _ = Task.Run(async () => {
                try {
                    await workItem(cancellationToken).ConfigureAwait(false);
                }
                catch (Exception e) {
                    OnError(e, workItem);
                }
            }, cancellationToken);
        }

    }
}
