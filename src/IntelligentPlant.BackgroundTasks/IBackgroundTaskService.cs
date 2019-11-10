using System;
using System.Threading;
using System.Threading.Tasks;

namespace IntelligentPlant.BackgroundTasks {

    /// <summary>
    /// Describes a service that can queue work items to be run in background tasks.
    /// </summary>
    public interface IBackgroundTaskService {

        /// <summary>
        /// Adds a synchronous work item to the queue.
        /// </summary>
        /// <param name="workItem">
        ///   The work item.
        /// </param>
        void QueueBackgroundWorkItem(Action<CancellationToken> workItem);

        /// <summary>
        /// Adds an asynchronous work item, to the queue.
        /// </summary>
        /// <param name="workItem">
        ///   The work item.
        /// </param>
        void QueueBackgroundWorkItem(Func<CancellationToken, Task> workItem);

    }

}
