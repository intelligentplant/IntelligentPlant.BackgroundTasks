using System;

namespace IntelligentPlant.BackgroundTasks {

    /// <summary>
    /// Options for <see cref="BackgroundTaskService"/>.
    /// </summary>
    public class BackgroundTaskServiceOptions {

        /// <summary>
        /// When <see langword="false"/>, attempting to register a work item while the service is 
        /// stopped will throw an exception.
        /// </summary>
        public bool AllowWorkItemRegistrationWhileStopped { get; set; }

        /// <summary>
        /// A callback that will be invoked when a work item is queued.
        /// </summary>
        public Action<BackgroundWorkItem>? OnEnqueued { get; set; }

        /// <summary>
        /// A callback that will be invoked when a work item is dequeued immediately prior to 
        /// running.
        /// </summary>
        public Action<BackgroundWorkItem>? OnDequeued { get; set; }

        /// <summary>
        /// A callback that will be invoked when a work item is running.
        /// </summary>
        public Action<BackgroundWorkItem>? OnRunning { get; set; }

        /// <summary>
        /// A callback that will be invoked when a work item completes successfully.
        /// </summary>
        public Action<BackgroundWorkItem>? OnCompleted { get; set; }

        /// <summary>
        /// A callback that will be invoked when when an error occurs while running a background 
        /// task.
        /// </summary>
        public Action<BackgroundWorkItem, Exception>? OnError { get; set; }

    }
}
