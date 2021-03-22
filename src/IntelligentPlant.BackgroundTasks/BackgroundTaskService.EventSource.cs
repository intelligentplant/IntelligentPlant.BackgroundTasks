using System;
using System.Diagnostics.Tracing;

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
        /// <remarks>
        ///   In .NET Standard 2.1 (i.e. .NET Core 3.0 or later), event counters are also 
        ///   available the total number and rate of processing of background work items. Note that
        ///   the counter values refer to all instances of <see cref="BackgroundTaskService"/>, 
        ///   rather than a single instance.
        /// </remarks>
        [EventSource(
            Name = DiagnosticsSourceName, 
            LocalizationResources = "IntelligentPlant.BackgroundTasks.EventSourceResources"
        )]
        public class BackgroundTaskServiceEventSource : EventSource {

#if NETSTANDARD2_1

            /// <summary>
            /// The current number of queued work items.
            /// </summary>
            private long _queueSize;

            /// <summary>
            /// The number of work items that are currently running.
            /// </summary>
            private long _workItemsRunning;

            /// <summary>
            /// The total number of work items that have finished running (both to completion and 
            /// faulted) since startup.
            /// </summary>
            private long _totalWorkItemsCompleted;

            /// <summary>
            /// The total number of work items that have finished successfully since startup.
            /// </summary>
            private long _totalSuccessfulWorkItems;

            /// <summary>
            /// The total number of work items that have finished due to an exception since startup.
            /// </summary>
            private long _totalWorkItemsFaulted;

            /// <summary>
            /// Counter that tracks to number of queued work items.
            /// </summary>
            private PollingCounter? _queueSizeCounter;

            /// <summary>
            /// Counter that tracks the number of work items that are currently running.
            /// </summary>
            private PollingCounter? _workItemsRunningCounter;

            /// <summary>
            /// Counter that tracks the total number of work items that have finished (successfully 
            /// or otherwise) since startup.
            /// </summary>
            private PollingCounter? _totalWorkItemsCompletedCounter;

            /// <summary>
            /// Counter that tracks the rate that work items are finishing at (successfully 
            /// or otherwise).
            /// </summary>
            private IncrementingPollingCounter? _workItemCompletedRateCounter;

            /// <summary>
            /// Counter that tracks the total number of work items that have completed successfully 
            /// since startup.
            /// </summary>
            private PollingCounter? _totalSuccessfulWorkItemsCounter;

            /// <summary>
            /// Counter that tracks the rate that work items are being completed successfully at.
            /// </summary>
            private IncrementingPollingCounter? _successfulWorkItemsRateCounter;

            /// <summary>
            /// Counter that tracks the total number of work items that have completed 
            /// unsuccessfully since startup.
            /// </summary>
            private PollingCounter? _totalWorkItemsFaultedCounter;

            /// <summary>
            /// Counter that tracks the rate that work items are being completed unsuccessfully at.
            /// </summary>
            private IncrementingPollingCounter? _faultedWorkItemsRateCounter;
#endif


            /// <summary>
            /// Creates a new <see cref="BackgroundTaskServiceEventSource"/> object.
            /// </summary>
            internal BackgroundTaskServiceEventSource() { }


#if NETSTANDARD2_1
            /// <inheritdoc/>
            protected override void OnEventCommand(EventCommandEventArgs command) {
                base.OnEventCommand(command);
                if (command.Command == EventCommand.Enable) {
                    _queueSizeCounter ??= new PollingCounter(EventSourceResources.Counter_QueueSize_Name, this, () => _queueSize) { 
                        DisplayName = EventSourceResources.Counter_QueueSize_DisplayName
                    };

                    _workItemsRunningCounter ??= new PollingCounter(EventSourceResources.Counter_RunningWorkItems_Name, this, () => _workItemsRunning) { 
                        DisplayName = EventSourceResources.Counter_RunningWorkItems_DisplayName
                    };

                    _totalWorkItemsCompletedCounter ??= new PollingCounter(EventSourceResources.Counter_TotalCompletedWorkItems_Name, this, () => _totalWorkItemsCompleted) { 
                        DisplayName = EventSourceResources.Counter_TotalCompletedWorkItems_DisplayName
                    };
                    _workItemCompletedRateCounter ??= new IncrementingPollingCounter(EventSourceResources.Counter_CompletedWorkItems_Name, this, () => _totalWorkItemsCompleted) { 
                        DisplayName = EventSourceResources.Counter_CompletedWorkItems_DisplayName
                    };
                    
                    _totalSuccessfulWorkItemsCounter ??= new PollingCounter(EventSourceResources.Counter_TotalSuccessfulWorkItems_Name, this, () => _totalSuccessfulWorkItems) { 
                        DisplayName = EventSourceResources.Counter_TotalSuccessfulWorkItems_DisplayName
                    };
                    _successfulWorkItemsRateCounter ??= new IncrementingPollingCounter(EventSourceResources.Counter_SuccessfulWorkItems_Name, this, () => _totalSuccessfulWorkItems) { 
                        DisplayName = EventSourceResources.Counter_SuccessfulWorkItems_DisplayName
                    };

                    _totalWorkItemsFaultedCounter ??= new PollingCounter(EventSourceResources.Counter_TotalFaultedWorkItems_Name, this, () => _totalWorkItemsFaulted) { 
                        DisplayName = EventSourceResources.Counter_TotalFaultedWorkItems_DisplayName
                    };
                    _faultedWorkItemsRateCounter ??= new IncrementingPollingCounter(EventSourceResources.Counter_FaultedWorkItems_Name, this, () => _totalWorkItemsFaulted) { 
                        DisplayName = EventSourceResources.Counter_FaultedWorkItems_DisplayName
                    };
                }
            }
#endif


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
            /// <param name="description">
            ///   The work item description.
            /// </param>
            /// <param name="queueSize">
            ///   The size of the work item queue.
            /// </param>
            [Event(EventIds.WorkItemEnqueued, Level = EventLevel.Informational)]
            public void WorkItemEnqueued(string serviceName, string id, string? description, int queueSize) {
#if NETSTANDARD2_1
                ++_queueSize;
#endif
                WriteEvent(EventIds.WorkItemEnqueued, serviceName, id, description, queueSize);
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
            /// <param name="description">
            ///   The work item description.
            /// </param>
            /// <param name="queueSize">
            ///   The size of the work item queue.
            /// </param>
            [Event(EventIds.WorkItemDequeued, Level = EventLevel.Informational)]
            public void WorkItemDequeued(string serviceName, string id, string? description, int queueSize) {
#if NETSTANDARD2_1
                --_queueSize;
#endif
                WriteEvent(EventIds.WorkItemDequeued, serviceName, id, description, queueSize);
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
            /// <param name="description">
            ///   The work item description.
            /// </param>
            [Event(EventIds.WorkItemRunning, Level = EventLevel.Informational)]
            public void WorkItemRunning(string serviceName, string id, string? description) {
#if NETSTANDARD2_1
                ++_workItemsRunning;
#endif
                WriteEvent(EventIds.WorkItemRunning, serviceName, id, description);
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
            /// <param name="description">
            ///   The work item description.
            /// </param>
            /// <param name="elapsed">
            ///   The elapsed time for the work item in seconds.
            /// </param>
            [Event(EventIds.WorkItemCompleted, Level = EventLevel.Informational)]
            public void WorkItemCompleted(string serviceName, string id, string? description, double elapsed) {
#if NETSTANDARD2_1
                --_workItemsRunning;
                ++_totalWorkItemsCompleted;
                ++_totalSuccessfulWorkItems;
#endif
                WriteEvent(EventIds.WorkItemCompleted, serviceName, id, description, elapsed);
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
            /// <param name="description">
            ///   The work item description.
            /// </param>
            /// <param name="elapsed">
            ///   The elapsed time for the work item in seconds.
            /// </param>
            [Event(EventIds.WorkItemFaulted, Level = EventLevel.Warning)]
            public void WorkItemFaulted(string serviceName, string id, string? description, double elapsed) {
#if NETSTANDARD2_1
                --_workItemsRunning;
                ++_totalWorkItemsCompleted;
                ++_totalWorkItemsFaulted;
#endif
                WriteEvent(EventIds.WorkItemFaulted, serviceName, id, description, elapsed);
            }

        }

    }
}
