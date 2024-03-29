﻿using System;

namespace IntelligentPlant.BackgroundTasks {

    /// <summary>
    /// Describes a service that can queue work items to be run in background tasks.
    /// </summary>
    /// <remarks>
    ///   Extension methods for enqueuing work items are defined in <see cref="BackgroundTaskServiceExtensions"/>.
    /// </remarks>
    /// <seealso cref="BackgroundTaskServiceExtensions"/>
    public interface IBackgroundTaskService : IDisposable {

        /// <summary>
        /// Gets a flag that indicates if the <see cref="IBackgroundTaskService"/> is currently 
        /// running.
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// Gets the number of work items that are currently queued.
        /// </summary>
        int QueuedItemCount { get; }

        /// <summary>
        /// Adds a work item to the queue.
        /// </summary>
        /// <param name="workItem">
        ///   The work item.
        /// </param>
        void QueueBackgroundWorkItem(BackgroundWorkItem workItem);

        /// <summary>
        /// Raised before a work item is started.
        /// </summary>
        event EventHandler<BackgroundWorkItem>? BeforeWorkItemStarted;

        /// <summary>
        /// Raised after a work item has completed.
        /// </summary>
        event EventHandler<BackgroundWorkItem>? AfterWorkItemCompleted;

    }

}
