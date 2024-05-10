using System;

using Microsoft.Extensions.Logging;

namespace IntelligentPlant.BackgroundTasks {
    partial class BackgroundTaskService {

        [LoggerMessage(EventIds.ServiceRunning, LogLevel.Information, "Background task service '{name}' is running.", EventName = nameof(EventIds.ServiceRunning))]
        static partial void LogServiceRunning(ILogger logger, string name);

        [LoggerMessage(EventIds.ServiceStopped, LogLevel.Information, "Background task service '{name}' has stopped.", EventName = nameof(EventIds.ServiceStopped))]
        static partial void LogServiceStopped(ILogger logger, string name);

        [LoggerMessage(EventIds.WorkItemEnqueued, LogLevel.Trace, "Work item enqueued on service '{name}': {workItem}", EventName = nameof(EventIds.WorkItemEnqueued))]
        static partial void LogWorkItemEnqueued(ILogger logger, string name, BackgroundWorkItem workItem);

        [LoggerMessage(EventIds.WorkItemDequeued, LogLevel.Trace, "Work item dequeued on service '{name}': {workItem}", EventName = nameof(EventIds.WorkItemDequeued))]
        static partial void LogWorkItemDequeued(ILogger logger, string name, BackgroundWorkItem workItem);

        [LoggerMessage(EventIds.WorkItemRunning, LogLevel.Trace, "Work item running on service '{name}': {workItem}", EventName = nameof(EventIds.WorkItemRunning))]
        static partial void LogWorkItemRunning(ILogger logger, string name, BackgroundWorkItem workItem);

        [LoggerMessage(EventIds.WorkItemCompleted, LogLevel.Trace, "Work item completed on service '{name}': {workItem}", EventName = nameof(EventIds.WorkItemCompleted))]
        static partial void LogWorkItemCompleted(ILogger logger, string name, BackgroundWorkItem workItem);

        [LoggerMessage(EventIds.WorkItemFaulted, LogLevel.Trace, "Work item faulted on service '{name}': {workItem}", EventName = nameof(EventIds.WorkItemFaulted))]
        static partial void LogWorkItemFaulted(ILogger logger, string name, BackgroundWorkItem workItem, Exception error);

        [LoggerMessage(EventIds.ErrorInCallback, LogLevel.Error, "Error in callback or event handler '{callback}' on background task service '{name}'. Handlers should use try..catch blocks to prevent errors from propagating to the background task service.", EventName = nameof(EventIds.ErrorInCallback))]
        static partial void LogErrorInCallback(ILogger logger, string name, string callback, Exception error);

    }
}
