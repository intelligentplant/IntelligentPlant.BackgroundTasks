using System;

using Microsoft.Extensions.Logging;

namespace IntelligentPlant.BackgroundTasks {
    partial class BackgroundTaskService {

        /// <summary>
        /// Service running log delegate.
        /// </summary>
        private static readonly Action<ILogger, Exception> s_onServiceRunning =
            LoggerMessage.Define(LogLevel.Information, new EventId(EventIds.ServiceRunning, nameof(EventIds.ServiceRunning)), Resources.Log_ServiceRunning);

        /// <summary>
        /// Service stopped log delegate.
        /// </summary>
        private static readonly Action<ILogger, Exception> s_onServiceStopped =
            LoggerMessage.Define(LogLevel.Information, new EventId(EventIds.ServiceStopped, nameof(EventIds.ServiceStopped)), Resources.Log_ServiceStopped);

        /// <summary>
        /// Work item enqueued log delegate.
        /// </summary>
        private static readonly Action<ILogger, BackgroundWorkItem, Exception> s_onItemEnqueued =
            LoggerMessage.Define<BackgroundWorkItem>(LogLevel.Trace, new EventId(EventIds.WorkItemEnqueued, nameof(EventIds.WorkItemEnqueued)), Resources.Log_ItemEnqueued);

        /// <summary>
        /// Work item enqueued while stopped log delegate.
        /// </summary>
        private static readonly Action<ILogger, BackgroundWorkItem, Exception> s_onItemEnqueuedWhileStopped =
            LoggerMessage.Define<BackgroundWorkItem>(LogLevel.Warning, new EventId(EventIds.WorkItemEnqueued, nameof(EventIds.WorkItemEnqueued)), Resources.Log_ItemEnqueuedWhileStopped);

        /// <summary>
        /// Work item dequeued log delegate.
        /// </summary>
        private static readonly Action<ILogger, BackgroundWorkItem, Exception> s_onItemDequeued =
            LoggerMessage.Define<BackgroundWorkItem>(LogLevel.Trace, new EventId(EventIds.WorkItemDequeued, nameof(EventIds.WorkItemDequeued)), Resources.Log_ItemDequeued);

        /// <summary>
        /// Work item running log delegate.
        /// </summary>
        private static readonly Action<ILogger, BackgroundWorkItem, Exception> s_onItemRunning =
            LoggerMessage.Define<BackgroundWorkItem>(LogLevel.Trace, new EventId(EventIds.WorkItemRunning, nameof(EventIds.WorkItemRunning)), Resources.Log_ItemRunning);

        /// <summary>
        /// Work item completed log delegate.
        /// </summary>
        private static readonly Action<ILogger, BackgroundWorkItem, Exception> s_onItemCompleted =
            LoggerMessage.Define<BackgroundWorkItem>(LogLevel.Trace, new EventId(EventIds.WorkItemCompleted, nameof(EventIds.WorkItemCompleted)), Resources.Log_ItemCompleted);

        /// <summary>
        /// Work item faulted log delegate.
        /// </summary>
        private static readonly Action<ILogger, BackgroundWorkItem, Exception> s_onItemFaulted =
            LoggerMessage.Define<BackgroundWorkItem>(LogLevel.Trace, new EventId(EventIds.WorkItemFaulted, nameof(EventIds.WorkItemFaulted)), Resources.Log_ItemFaulted);


        /// <summary>
        /// Writes a log message specifying that the service is running.
        /// </summary>
        /// <param name="logger">
        ///   The logger.
        /// </param>
        private static void LogServiceRunning(ILogger logger) {
            s_onServiceRunning(logger, null!);
        }


        /// <summary>
        /// Writes a log message specifying that the service has stopped.
        /// </summary>
        /// <param name="logger">
        ///   The logger.
        /// </param>
        private static void LogServiceStopped(ILogger logger) {
            s_onServiceStopped(logger, null!);
        }


        /// <summary>
        /// Writes a log message specifying that a work item has been enqueued.
        /// </summary>
        /// <param name="logger">
        ///   The logger.
        /// </param>
        /// <param name="workitem">
        ///   The work item.
        /// </param>
        /// <param name="isRunning">
        ///   <see langword="true"/> if the background task service is currently running, or 
        ///   <see langword="false"/> if it is stopped.
        /// </param>
        private static void LogItemEnqueued(ILogger logger, BackgroundWorkItem workitem, bool isRunning) {
            if (isRunning) {
                s_onItemEnqueued(logger, workitem, null!);
            }
            else {
                s_onItemEnqueuedWhileStopped(logger, workitem, null!);
            }
        }


        /// <summary>
        /// Writes a log message specifying that a work item has been dequeued.
        /// </summary>
        /// <param name="logger">
        ///   The logger.
        /// </param>
        /// <param name="workitem">
        ///   The work item.
        /// </param>
        private static void LogItemDequeued(ILogger logger, BackgroundWorkItem workitem) {
            s_onItemDequeued(logger, workitem, null!);
        }


        /// <summary>
        /// Writes a log message specifying that a work item is running.
        /// </summary>
        /// <param name="logger">
        ///   The logger.
        /// </param>
        /// <param name="workitem">
        ///   The work item.
        /// </param>
        private static void LogItemRunning(ILogger logger, BackgroundWorkItem workitem) {
            s_onItemRunning(logger, workitem, null!);
        }


        /// <summary>
        /// Writes a log message specifying that a work item has completed.
        /// </summary>
        /// <param name="logger">
        ///   The logger.
        /// </param>
        /// <param name="workitem">
        ///   The work item.
        /// </param>
        private static void LogItemCompleted(ILogger logger, BackgroundWorkItem workitem) {
            s_onItemCompleted(logger, workitem, null!);
        }


        /// <summary>
        /// Writes a log message specifying that a work item has faulted.
        /// </summary>
        /// <param name="logger">
        ///   The logger.
        /// </param>
        /// <param name="workitem">
        ///   The work item.
        /// </param>
        /// <param name="error">
        ///   The error that caused the fault.
        /// </param>
        private static void LogItemFaulted(ILogger logger, BackgroundWorkItem workitem, Exception error) {
            s_onItemFaulted(logger, workitem, error);
        }

    }
}
