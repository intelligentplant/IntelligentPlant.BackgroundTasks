using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Diagnostics.Tracing;
using System.Linq;

namespace IntelligentPlant.BackgroundTasks {
    partial class BackgroundTaskService {

        /// <summary>
        /// Contains event IDs for use in log messages and <see cref="EventSource"/> events.
        /// </summary>
        public static class EventIds {

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
            Name = DiagnosticsSourceName, 
            LocalizationResources = "IntelligentPlant.BackgroundTasks.EventSourceResources"
        )]
        public class BackgroundTaskServiceEventSource : EventSource {

            /// <summary>
            /// Instrumentation meter.
            /// </summary>
            private static readonly Meter s_meter = new Meter(DiagnosticsSourceName);

            /// <summary>
            /// Holds queue sizes emitted by <see cref="s_queueSizeCounter"/>. Cleared on every 
            /// call to observe values.
            /// </summary>
            private static readonly Dictionary<string, int> s_queueSizes = new Dictionary<string, int>(StringComparer.Ordinal);

            /// <summary>
            /// Counter that emits the total number of work items that have been queued by 
            /// background task services.
            /// </summary>
            private static readonly Counter<long> s_queuedItemsCounter = s_meter.CreateCounter<long>("Total_Queued_Items", "{work items}", "Number of work items that have been queued since observation of the counter began.");

            /// <summary>
            /// Counter that emits the total number of work items that have been dequeued by 
            /// background task services in preparation for running.
            /// </summary>
            private static readonly Counter<long> s_dequeuedItemsCounter = s_meter.CreateCounter<long>("Total_Dequeued_Items", "{work items}", "Number of work items that have been dequeued for execution since observation of the counter began.");

            /// <summary>
            /// Counter that emits the total number of work items that have started running.
            /// </summary>
            private static readonly Counter<long> s_startedItemsCounter = s_meter.CreateCounter<long>("Total_Started_Items", "{work items}", "Number of work items that have started executing since observation of the counter began.");

            /// <summary>
            /// Counter that emits the number of work items that are currently running.
            /// </summary>
            private static readonly Counter<long> s_currentRunningItemsCounter = s_meter.CreateCounter<long>("Running_Items", "{work items}", "Number of work items that are currently running.");

            /// <summary>
            /// Counter that emits the total number of work items that have completed.
            /// </summary>
            private static readonly Counter<long> s_completedItemsCounter = s_meter.CreateCounter<long>("Total_Completed_Items", "{work items}", "Number work items that have completed since observation of the counter began.");

            /// <summary>
            /// Counter that emits the total number of work items that have completed successfully.
            /// </summary>
            private static readonly Counter<long> s_completedItemsSuccessCounter = s_meter.CreateCounter<long>("Total_Completed_Items_Success", "{work items}", "Number of successfully completed work items since observation of the counter began.");

            /// <summary>
            /// Counter that emits the total number of work items that have faulted.
            /// </summary>
            private static readonly Counter<long> s_completedItemsFaultedCounter = s_meter.CreateCounter<long>("Total_Completed_Items_Fail", "{work items}", "Number of work items that completed with a fault since observation of the counter began.");

            /// <summary>
            /// Histogram that emits the duration that work items ran for.
            /// </summary>
            private static readonly Histogram<double> s_processingTimeCounter = s_meter.CreateHistogram<double>("Processing_Time", "s", "Time to complete a work item.");
            
            /// <summary>
            /// Gauge that emits the current queue size for all background task services.
            /// </summary>
            private static readonly ObservableGauge<int> s_queueSizeCounter = s_meter.CreateObservableGauge<int>("Queue_Size", () => {
                KeyValuePair<string, int>[] vals;
                
                lock (s_queueSizes) {
                    vals = s_queueSizes.ToArray();
                    s_queueSizes.Clear();
                }

                return vals.Select(x => new Measurement<int>(x.Value, new KeyValuePair<string, object?>(ServiceNameTag, x.Key))).ToArray();
            }, "{work items}", "Current number of pending work items.");

            /// <summary>
            /// Tag added to counter values to identify the background task service that the 
            /// counter applies to.
            /// </summary>
            private const string ServiceNameTag = DiagnosticsSourceName + ".Service_Name";


            /// <summary>
            /// Creates a new <see cref="BackgroundTaskServiceEventSource"/> object.
            /// </summary>
            internal BackgroundTaskServiceEventSource() { }


            /// <summary>
            /// Writes an <see cref="EventIds.ServiceRunning"/> event.
            /// </summary>
            /// <param name="serviceName">
            ///   The name of the service.
            /// </param>
            [Event(EventIds.ServiceRunning, Level = EventLevel.LogAlways)]
            public void ServiceRunning(string serviceName) {
                WriteEvent(EventIds.ServiceRunning, serviceName);
            }


            /// <summary>
            /// Writes an <see cref="EventIds.ServiceStopped"/> event.
            /// </summary>
            /// <param name="serviceName">
            ///   The name of the service.
            /// </param>
            [Event(EventIds.ServiceStopped, Level = EventLevel.LogAlways)]
            public void ServiceStopped(string serviceName) {
                WriteEvent(EventIds.ServiceStopped, serviceName);
            }


            /// <summary>
            /// Writes an <see cref="EventIds.WorkItemEnqueued"/> event.
            /// </summary>
            /// <param name="serviceName">
            ///   The name of the service.
            /// </param>
            /// <param name="id">
            ///   The work item ID.
            /// </param>
            /// <param name="displayName">
            ///   The work item display name.
            /// </param>
            /// <param name="queueSize">
            ///   The size of the work item queue.
            /// </param>
            [Event(EventIds.WorkItemEnqueued, Level = EventLevel.Informational)]
            public void WorkItemEnqueued(string serviceName, string id, string? displayName, int queueSize) {
                WriteEvent(EventIds.WorkItemEnqueued, serviceName, id, displayName, queueSize);
                if (s_queuedItemsCounter.Enabled) {
                    s_queuedItemsCounter.Add(1, new KeyValuePair<string, object?>(ServiceNameTag, serviceName));
                }
                if (s_queuedItemsCounter.Enabled) { 
                    lock (s_queueSizes) {
                        s_queueSizes[serviceName] = queueSize;
                    }
                }
            }


            /// <summary>
            /// Writes an <see cref="EventIds.WorkItemDequeued"/> event.
            /// </summary>
            /// <param name="serviceName">
            ///   The name of the service.
            /// </param>
            /// <param name="id">
            ///   The work item ID.
            /// </param>
            /// <param name="displayName">
            ///   The work item display name.
            /// </param>
            /// <param name="queueSize">
            ///   The size of the work item queue.
            /// </param>
            [Event(EventIds.WorkItemDequeued, Level = EventLevel.Informational)]
            public void WorkItemDequeued(string serviceName, string id, string? displayName, int queueSize) {
                WriteEvent(EventIds.WorkItemDequeued, serviceName, id, displayName, queueSize);
                if (s_dequeuedItemsCounter.Enabled) {
                    s_dequeuedItemsCounter.Add(1, new KeyValuePair<string, object?>(ServiceNameTag, serviceName));
                }
                if (s_queuedItemsCounter.Enabled) {
                    lock (s_queueSizes) {
                        s_queueSizes[serviceName] = queueSize;
                    }
                }
            }


            /// <summary>
            /// Writes an <see cref="EventIds.WorkItemRunning"/> event.
            /// </summary>
            /// <param name="serviceName">
            ///   The name of the service.
            /// </param>
            /// <param name="id">
            ///   The work item ID.
            /// </param>
            /// <param name="displayName">
            ///   The work item display name.
            /// </param>
            [Event(EventIds.WorkItemRunning, Level = EventLevel.Informational)]
            public void WorkItemRunning(string serviceName, string id, string? displayName) {
                WriteEvent(EventIds.WorkItemRunning, serviceName, id, displayName);

                KeyValuePair<string, object?>? tag = null;
                
                if (s_startedItemsCounter.Enabled) {
                    s_startedItemsCounter.Add(1, tag ??= new KeyValuePair<string, object?>(ServiceNameTag, serviceName));
                }
                if (s_currentRunningItemsCounter.Enabled) {
                    s_currentRunningItemsCounter.Add(1, tag ??= new KeyValuePair<string, object?>(ServiceNameTag, serviceName));
                }
            }


            /// <summary>
            /// Writes an <see cref="EventIds.WorkItemCompleted"/> event.
            /// </summary>
            /// <param name="serviceName">
            ///   The name of the service.
            /// </param>
            /// <param name="id">
            ///   The work item ID.
            /// </param>
            /// <param name="displayName">
            ///   The work item display name.
            /// </param>
            /// <param name="elapsed">
            ///   The elapsed time for the work item in seconds.
            /// </param>
            [Event(EventIds.WorkItemCompleted, Level = EventLevel.Informational)]
            public void WorkItemCompleted(string serviceName, string id, string? displayName, double elapsed) {
                WriteEvent(EventIds.WorkItemCompleted, serviceName, id, displayName, elapsed);

                KeyValuePair<string, object?>? tag = null;

                if (s_completedItemsCounter.Enabled) {
                    s_completedItemsCounter.Add(1, tag ??= new KeyValuePair<string, object?>(ServiceNameTag, serviceName));
                }
                if (s_completedItemsSuccessCounter.Enabled) {
                    s_completedItemsSuccessCounter.Add(1, tag ??= new KeyValuePair<string, object?>(ServiceNameTag, serviceName));
                }
                if (s_processingTimeCounter.Enabled) {
                    s_processingTimeCounter.Record(elapsed, tag ??= new KeyValuePair<string, object?>(ServiceNameTag, serviceName));
                }
                if (s_currentRunningItemsCounter.Enabled) {
                    s_currentRunningItemsCounter.Add(-1, tag ??= new KeyValuePair<string, object?>(ServiceNameTag, serviceName));
                }
            }


            /// <summary>
            /// Writes an <see cref="EventIds.WorkItemFaulted"/> event.
            /// </summary>
            /// <param name="serviceName">
            ///   The name of the service.
            /// </param>
            /// <param name="id">
            ///   The work item ID.
            /// </param>
            /// <param name="displayName">
            ///   The work item display name.
            /// </param>
            /// <param name="elapsed">
            ///   The elapsed time for the work item in seconds.
            /// </param>
            [Event(EventIds.WorkItemFaulted, Level = EventLevel.Warning)]
            public void WorkItemFaulted(string serviceName, string id, string? displayName, double elapsed) {
                WriteEvent(EventIds.WorkItemFaulted, serviceName, id, displayName, elapsed);

                KeyValuePair<string, object?>? tag = null;

                if (s_completedItemsCounter.Enabled) {
                    s_completedItemsCounter.Add(1, tag ??= new KeyValuePair<string, object?>(ServiceNameTag, serviceName));
                }
                if (s_completedItemsFaultedCounter.Enabled) {
                    s_completedItemsFaultedCounter.Add(1, tag ??= new KeyValuePair<string, object?>(ServiceNameTag, serviceName));
                }
                if (s_processingTimeCounter.Enabled) {
                    s_processingTimeCounter.Record(elapsed, tag ??= new KeyValuePair<string, object?>(ServiceNameTag, serviceName));
                }
                if (s_currentRunningItemsCounter.Enabled) {
                    s_currentRunningItemsCounter.Add(-1, tag ??= new KeyValuePair<string, object?>(ServiceNameTag, serviceName));
                }
            }

        }

    }
}
