using System;

using Microsoft.Extensions.Logging;

namespace IntelligentPlant.BackgroundTasks {
    partial class BackgroundTaskService {

        [LoggerMessage(EventIds.ServiceRunning, LogLevel.Information, "Background task service is running.", EventName = nameof(EventIds.ServiceRunning))]
        static partial void LogServiceRunning(ILogger logger);

        [LoggerMessage(EventIds.ServiceStopped, LogLevel.Information, "Background task service has stopped.", EventName = nameof(EventIds.ServiceStopped))]
        static partial void LogServiceStopped(ILogger logger);

        [LoggerMessage(EventIds.WorkItemEnqueued, LogLevel.Trace, "Work item enqueued: {workItem}", EventName = nameof(EventIds.WorkItemEnqueued))]
        static partial void LogWorkItemEnqueued(ILogger logger, BackgroundWorkItem workItem);

        [LoggerMessage(EventIds.WorkItemDequeued, LogLevel.Trace, "Work item dequeued: {workItem}", EventName = nameof(EventIds.WorkItemDequeued))]
        static partial void LogWorkItemDequeued(ILogger logger, BackgroundWorkItem workItem);

        [LoggerMessage(EventIds.WorkItemRunning, LogLevel.Trace, "Work item running: {workItem}", EventName = nameof(EventIds.WorkItemRunning))]
        static partial void LogWorkItemRunning(ILogger logger, BackgroundWorkItem workItem);

        [LoggerMessage(EventIds.WorkItemCompleted, LogLevel.Trace, "Work item completed: {workItem}", EventName = nameof(EventIds.WorkItemCompleted))]
        static partial void LogWorkItemCompleted(ILogger logger, BackgroundWorkItem workItem);

        [LoggerMessage(EventIds.WorkItemFaulted, LogLevel.Trace, "Work item faulted: {workItem}", EventName = nameof(EventIds.WorkItemFaulted))]
        static partial void LogWorkItemFaulted(ILogger logger, BackgroundWorkItem workItem, Exception error);

        [LoggerMessage(EventIds.ErrorInCallback, LogLevel.Error, "Error in callback or event handler '{callback}'. Handlers should use try..catch blocks to prevent errors from propagating to the background task service.", EventName = nameof(EventIds.ErrorInCallback))]
        static partial void LogErrorInCallback(ILogger logger, string callback, Exception error);

    }
}
