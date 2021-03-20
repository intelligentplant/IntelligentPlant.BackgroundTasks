﻿using System;
using System.Diagnostics.Tracing;

namespace IntelligentPlant.BackgroundTasks {
    partial class BackgroundTaskService {

        /// <summary>
        /// Contains event codes for use in log messages and <see cref="EventSource"/> events.
        /// </summary>
        public static class EventCodes {

            /// <summary>
            /// Service is running.
            /// </summary>
            public const int ServiceRunning = 1;

            /// <summary>
            /// Service has stopped.
            /// </summary>
            public const int ServiceStopped = 2;

            /// <summary>
            /// A work item has been enqueued.
            /// </summary>
            public const int WorkItemEnqueued = 3;

            /// <summary>
            /// A work item has been dequeued.
            /// </summary>
            public const int WorkItemDequeued = 4;

            /// <summary>
            /// A work item is running.
            /// </summary>
            public const int WorkItemRunning = 5;

            /// <summary>
            /// A work item has been completed.
            /// </summary>
            public const int WorkItemCompleted = 6;

            /// <summary>
            /// A work item has faulted.
            /// </summary>
            public const int WorkItemFaulted = 7;

        }


        /// <summary>
        /// <see cref="System.Diagnostics.Tracing.EventSource"/> for a <see cref="BackgroundTaskService"/>.
        /// </summary>
        [EventSource(
            Name = "IntelligentPlant.BackgroundTasks", 
            LocalizationResources = "IntelligentPlant.BackgroundTasks.EventSourceResources"
        )]
        public class BackgroundTaskServiceEventSource : EventSource {

            /// <summary>
            /// Creates a new <see cref="BackgroundTaskServiceEventSource"/> object.
            /// </summary>
            internal BackgroundTaskServiceEventSource() { }


            /// <summary>
            /// Writes an <see cref="EventCodes.ServiceRunning"/> event.
            /// </summary>
            [Event(EventCodes.ServiceRunning, Level = EventLevel.LogAlways)]
            public void ServiceRunning() {
                WriteEvent(EventCodes.ServiceRunning);
            }


            /// <summary>
            /// Writes an <see cref="EventCodes.ServiceStopped"/> event.
            /// </summary>
            [Event(EventCodes.ServiceStopped, Level = EventLevel.LogAlways)]
            public void ServiceStopped() {
                WriteEvent(EventCodes.ServiceStopped);
            }


            /// <summary>
            /// Writes an <see cref="EventCodes.WorkItemEnqueued"/> event.
            /// </summary>
            /// <param name="id">
            ///   The work item ID.
            /// </param>
            /// <param name="description">
            ///   The work item description.
            /// </param>
            /// <param name="queueSize">
            ///   The size of the work item queue.
            /// </param>
            [Event(EventCodes.WorkItemEnqueued, Level = EventLevel.Informational)]
            public void WorkItemEnqueued(Guid id, string? description, int queueSize) {
                WriteEvent(EventCodes.WorkItemEnqueued, id, description, queueSize);
            }


            /// <summary>
            /// Writes an <see cref="EventCodes.WorkItemDequeued"/> event.
            /// </summary>
            /// <param name="id">
            ///   The work item ID.
            /// </param>
            /// <param name="description">
            ///   The work item description.
            /// </param>
            /// <param name="queueSize">
            ///   The size of the work item queue.
            /// </param>
            [Event(EventCodes.WorkItemDequeued, Level = EventLevel.Informational)]
            public void WorkItemDequeued(Guid id, string? description, int queueSize) {
                WriteEvent(EventCodes.WorkItemDequeued, id, description, queueSize);
            }


            /// <summary>
            /// Writes an <see cref="EventCodes.WorkItemRunning"/> event.
            /// </summary>
            /// <param name="id">
            ///   The work item ID.
            /// </param>
            /// <param name="description">
            ///   The work item description.
            /// </param>
            [Event(EventCodes.WorkItemRunning, Level = EventLevel.Informational)]
            public void WorkItemRunning(Guid id, string? description) {
                WriteEvent(EventCodes.WorkItemRunning, id, description);
            }


            /// <summary>
            /// Writes an <see cref="EventCodes.WorkItemCompleted"/> event.
            /// </summary>
            /// <param name="id">
            ///   The work item ID.
            /// </param>
            /// <param name="description">
            ///   The work item description.
            /// </param>
            /// <param name="elapsed">
            ///   The elapsed time for the work item.
            /// </param>
            [Event(EventCodes.WorkItemCompleted, Level = EventLevel.Informational)]
            public void WorkItemCompleted(Guid id, string? description, string elapsed) {
                WriteEvent(EventCodes.WorkItemCompleted, id, description, elapsed);
            }


            /// <summary>
            /// Writes an <see cref="EventCodes.WorkItemFaulted"/> event.
            /// </summary>
            /// <param name="id">
            ///   The work item ID.
            /// </param>
            /// <param name="description">
            ///   The work item description.
            /// </param>
            /// <param name="elapsed">
            ///   The elapsed time for the work item.
            /// </param>
            [Event(EventCodes.WorkItemFaulted, Level = EventLevel.Warning)]
            public void WorkItemFaulted(Guid id, string? description, string elapsed) {
                WriteEvent(EventCodes.WorkItemFaulted, id, description, elapsed);
            }

        }

    }
}
