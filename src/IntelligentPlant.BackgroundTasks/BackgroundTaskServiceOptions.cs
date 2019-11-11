using System;

namespace IntelligentPlant.BackgroundTasks {

    /// <summary>
    /// Options for <see cref="BackgroundTaskService"/>.
    /// </summary>
    public class BackgroundTaskServiceOptions {

        /// <summary>
        /// A callback that will be invoked when when an error occurs while running a background 
        /// task.
        /// </summary>
        public Action<Exception, BackgroundWorkItem> OnError { get; set; }

    }
}
