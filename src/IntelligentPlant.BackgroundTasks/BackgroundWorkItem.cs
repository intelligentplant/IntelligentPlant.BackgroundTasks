using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace IntelligentPlant.BackgroundTasks {

    /// <summary>
    /// Describes a work item that has been added to an <see cref="IBackgroundTaskService"/> queue.
    /// </summary>
    /// <remarks>
    /// 
    /// <para>
    ///   Although <see cref="BackgroundWorkItem"/> implements <see cref="IDisposable"/>, you 
    ///   should not disposed of any instances you create yourself. The <see cref="IBackgroundTaskService"/> 
    ///   will dispose of work items once they have run to completion of have faulted.
    /// </para>
    /// 
    /// <para>
    ///   Both <see cref="WorkItem"/> and <see cref="WorkItemAsync"/> accept an <see cref="System.Diagnostics.Activity"/> 
    ///   parameter. This can be used to create OpenTelemetry-compatible activities inside the 
    ///   work item, or to augment the work item activity with tags, events and so on. Note that 
    ///   the parameter value will be <see langword="null"/> if there are no listeners for the 
    ///   <see cref="BackgroundTaskService.ActivitySource"/> source.
    /// </para>
    /// 
    /// </remarks>
    public struct BackgroundWorkItem : IEquatable<BackgroundWorkItem>, IDisposable {

        /// <summary>
        /// Gets the unique identifier for the work item.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Gets the optional description for the work item.
        /// </summary>
        public string? Description { get; }

        /// <summary>
        /// The synchronous work item. The value will be <see langword="null"/> if an asynchronous 
        /// work item was enqueued.
        /// </summary>
        public Action<Activity?, CancellationToken>? WorkItem { get; }

        /// <summary>
        /// The asynchronous work item. The value will be <see langword="null"/> if a synchronous 
        /// work item was enqueued.
        /// </summary>
        public Func<Activity?, CancellationToken, Task>? WorkItemAsync { get; }

        /// <summary>
        /// The <see cref="System.Diagnostics.Activity"/> associated with the work item. Use this 
        /// activity as the parent when creating new child activities inside the <see cref="WorkItem"/> 
        /// or <see cref="WorkItemAsync"/> delegate.
        /// </summary>
        public Activity? Activity { get; }


        /// <summary>
        /// Creates a new <see cref="BackgroundWorkItem"/> with a synchronous work item.
        /// </summary>
        /// <param name="workItem">
        ///   The work item.
        /// </param>
        /// <param name="description">
        ///   The optional description for the work item. if no description is specified, a 
        ///   description will be constructed using the <see cref="MethodInfo"/> for the 
        ///   <paramref name="workItem"/>
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="workItem"/> is <see langword="null"/>.
        /// </exception>
        public BackgroundWorkItem(Action<Activity?, CancellationToken> workItem, string? description = null) {
            Activity = BackgroundTaskService.ActivitySource.StartActivity("background_task_sync");
            Id = Activity?.Id ?? Guid.NewGuid().ToString();
            WorkItem = workItem ?? throw new ArgumentNullException(nameof(workItem));
            WorkItemAsync = null;
            Description = description;

            if (!string.IsNullOrWhiteSpace(description)) {
                Activity?.AddTag(BackgroundTaskService.GetOpenTelemetryTagName("description"), description);
            }
        }


        /// <summary>
        /// Creates a new <see cref="BackgroundWorkItem"/> with an asynchronous work item.
        /// </summary>
        /// <param name="workItem">
        ///   The work item.
        /// </param>
        /// <param name="description">
        ///   The optional description for the work item. if no description is specified, a 
        ///   description will be constructed using the <see cref="MethodInfo"/> for the 
        ///   <paramref name="workItem"/>
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="workItem"/> is <see langword="null"/>.
        /// </exception>
        public BackgroundWorkItem(Func<Activity?, CancellationToken, Task> workItem, string? description = null) {
            Activity = BackgroundTaskService.ActivitySource.StartActivity("background_task_async");
            Id = Activity?.Id ?? Guid.NewGuid().ToString();
            WorkItem = null;
            WorkItemAsync = workItem ?? throw new ArgumentNullException(nameof(workItem));
            Description = description;

            if (!string.IsNullOrWhiteSpace(description)) {
                Activity?.AddTag(BackgroundTaskService.GetOpenTelemetryTagName("description"), description);
            }
        }


        /// <inheritdoc/>
        public override string ToString() {
            return string.Format(
                System.Globalization.CultureInfo.CurrentCulture,
                Resources.BackgroundWorkItem_StringFormat,
                Id,
                WorkItem != null
                    ? Resources.BackgroundWorkItem_ItemType_Sync
                    : WorkItemAsync != null
                        ? Resources.BackgroundWorkItem_ItemType_Async
                        : Resources.BackgroundWorkItem_ItemType_Undefined,
                Description
            );
        }


        /// <inheritdoc/>
        public override int GetHashCode() {
#if NETSTANDARD2_1 == null
            // Implementation from https://stackoverflow.com/questions/263400/what-is-the-best-algorithm-for-overriding-gethashcode/263416#263416
            unchecked {
                var hash = (int) 2166136261;
                hash = (hash * 16777619) ^ Id.GetHashCode();
                if (WorkItem != null) {
                    hash = (hash * 16777619) ^ WorkItem.GetHashCode();
                }
                if (WorkItemAsync != null) {
                    hash = (hash * 16777619) ^ WorkItemAsync.GetHashCode();
                }
                if (Description != null) {
                    hash = (hash * 16777619) ^ Description.GetHashCode();
                }
                return hash;
            }
#else
            return HashCode.Combine(Id, WorkItem, WorkItemAsync, Description);
#endif
        }


        /// <inheritdoc/>
        public override bool Equals(object obj) {
            return (obj is BackgroundWorkItem workItem)
                ? Equals(workItem)
                : false;
        }


        /// <inheritdoc/>
        public bool Equals(BackgroundWorkItem other) {
            return other.Id.Equals(Id, StringComparison.Ordinal) && 
                string.Equals(other.Description, Description, StringComparison.Ordinal) && 
                other.WorkItem == WorkItem && 
                other.WorkItemAsync == WorkItemAsync;
        }


        /// <inheritdoc/>
        public void Dispose() {
            Activity?.Stop();
        }


        /// <inheritdoc/>
        public static bool operator ==(BackgroundWorkItem left, BackgroundWorkItem right) {
            return left.Equals(right);
        }


        /// <inheritdoc/>
        public static bool operator !=(BackgroundWorkItem left, BackgroundWorkItem right) {
            return !(left == right);
        }

    }
}
