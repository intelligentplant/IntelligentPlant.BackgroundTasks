using System;

using Microsoft.Extensions.Logging;

namespace IntelligentPlant.BackgroundTasks {
    public partial class BackgroundTaskService {

        /// <summary>
        /// Service running log delegate.
        /// </summary>
        private static readonly Action<ILogger, Exception> s_onServiceRunning =
            LoggerMessage.Define(LogLevel.Information, new EventId(1, "ServiceRunning"), Resources.Log_ServiceRunning);

        /// <summary>
        /// Service stopped log delegate.
        /// </summary>
        private static readonly Action<ILogger, Exception> s_onServiceStopped =
            LoggerMessage.Define(LogLevel.Information, new EventId(2, "ServiceStopped"), Resources.Log_ServiceStopped);

        /// <summary>
        /// Work item enqueued log delegate.
        /// </summary>
        private static readonly Action<ILogger, BackgroundWorkItem, Exception> s_onItemEnqueued =
            LoggerMessage.Define<BackgroundWorkItem>(LogLevel.Trace, new EventId(3, "WorkItemEnqueued"), Resources.Log_ItemEnqueued);

        /// <summary>
        /// Work item dequeued log delegate.
        /// </summary>
        private static readonly Action<ILogger, BackgroundWorkItem, Exception> s_onItemDequeued =
            LoggerMessage.Define<BackgroundWorkItem>(LogLevel.Trace, new EventId(4, "WorkItemDequeued"), Resources.Log_ItemDequeued);

        /// <summary>
        /// Work item running log delegate.
        /// </summary>
        private static readonly Action<ILogger, BackgroundWorkItem, Exception> s_onItemRunning =
            LoggerMessage.Define<BackgroundWorkItem>(LogLevel.Trace, new EventId(5, "WorkItemRunning"), Resources.Log_ItemRunning);

        /// <summary>
        /// Work item completed log delegate.
        /// </summary>
        private static readonly Action<ILogger, BackgroundWorkItem, Exception> s_onItemCompleted =
            LoggerMessage.Define<BackgroundWorkItem>(LogLevel.Trace, new EventId(6, "WorkItemQueued"), Resources.Log_ItemCompleted);

        /// <summary>
        /// Work item faulted log delegate.
        /// </summary>
        private static readonly Action<ILogger, BackgroundWorkItem, Exception> s_onItemFaulted =
            LoggerMessage.Define<BackgroundWorkItem>(LogLevel.Trace, new EventId(7, "WorkItemQueued"), Resources.Log_ItemFaulted);


        /// <summary>
        /// Writes a log message specifying that the service is running.
        /// </summary>
        /// <param name="logger">
        ///   The logger.
        /// </param>
        private static void LogServiceRunning(ILogger logger) {
            s_onServiceRunning(logger, null);
        }


        /// <summary>
        /// Writes a log message specifying that the service has stopped.
        /// </summary>
        /// <param name="logger">
        ///   The logger.
        /// </param>
        private static void LogServiceStopped(ILogger logger) {
            s_onServiceStopped(logger, null);
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
        private static void LogItemEnqueued(ILogger logger, BackgroundWorkItem workitem) {
            s_onItemEnqueued(logger, workitem, null);
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
            s_onItemDequeued(logger, workitem, null);
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
            s_onItemRunning(logger, workitem, null);
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
            s_onItemCompleted(logger, workitem, null);
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
