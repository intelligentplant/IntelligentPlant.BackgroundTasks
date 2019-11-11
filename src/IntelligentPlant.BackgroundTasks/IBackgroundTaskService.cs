using System;
using System.Threading;
using System.Threading.Tasks;

namespace IntelligentPlant.BackgroundTasks {

    /// <summary>
    /// Describes a service that can queue work items to be run in background tasks.
    /// </summary>
    /// <remarks>
    ///   Extension methods for enqueuing work items are defined in <see cref="BackgroundTaskServiceExtensions"/>.
    /// </remarks>
    /// <seealso cref="BackgroundTaskServiceExtensions"/>
    public interface IBackgroundTaskService {

        /// <summary>
        /// Adds a work item to the queue.
        /// </summary>
        /// <param name="workItem">
        ///   The work item.
        /// </param>
        void QueueBackgroundWorkItem(BackgroundWorkItem workItem);

    }

}
