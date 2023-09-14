using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace IntelligentPlant.BackgroundTasks {

    /// <summary>
    /// Describes a work item that has been added to an <see cref="IBackgroundTaskService"/> queue.
    /// </summary>
    public struct BackgroundWorkItem : IEquatable<BackgroundWorkItem> {

        /// <summary>
        /// Gets the unique identifier for the work item.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// The display name for the work item.
        /// </summary>
        public string? DisplayName { get; }

        /// <summary>
        /// The synchronous work item. The value will be <see langword="null"/> if an asynchronous 
        /// work item was enqueued.
        /// </summary>
        public Action<CancellationToken>? WorkItem { get; }

        /// <summary>
        /// The asynchronous work item. The value will be <see langword="null"/> if a synchronous 
        /// work item was enqueued.
        /// </summary>
        public Func<CancellationToken, Task>? WorkItemAsync { get; }


        /// <summary>
        /// Creates a new <see cref="BackgroundWorkItem"/> with a synchronous work item.
        /// </summary>
        /// <param name="workItem">
        ///   The work item.
        /// </param>
        /// <param name="displayName">
        ///   The display name for the work item.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="workItem"/> is <see langword="null"/>.
        /// </exception>
        public BackgroundWorkItem(Action<CancellationToken> workItem, string? displayName = null) {
            WorkItem = workItem ?? throw new ArgumentNullException(nameof(workItem));
            WorkItemAsync = null;

            Id = Guid.NewGuid().ToString();
            DisplayName = displayName;
        }


        /// <summary>
        /// Creates a new <see cref="BackgroundWorkItem"/> with an asynchronous work item.
        /// </summary>
        /// <param name="workItem">
        ///   The work item.
        /// </param>
        /// <param name="displayName">
        ///   The display name for the work item.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="workItem"/> is <see langword="null"/>.
        /// </exception>
        public BackgroundWorkItem(Func<CancellationToken, Task> workItem, string? displayName = null) {
            WorkItem = null;
            WorkItemAsync = workItem ?? throw new ArgumentNullException(nameof(workItem));

            Id = Guid.NewGuid().ToString();
            DisplayName = displayName;
        }


        /// <summary>
        /// Creates a new <see cref="BackgroundWorkItem"/> with an asynchronous work item.
        /// </summary>
        /// <param name="workItem">
        ///   The synchronous work item.
        /// </param>
        /// <param name="workItemAsync">
        ///   The asynchronous work item.
        /// </param>
        /// <param name="id">
        ///   The work item ID.
        /// </param>
        /// <param name="displayName">
        ///   The display name for the work item.
        /// </param>
        internal BackgroundWorkItem(
            Action<CancellationToken>? workItem, 
            Func<CancellationToken, Task>? workItemAsync, 
            string id,
            string? displayName
        ) {
            WorkItem = workItem;
            WorkItemAsync = workItemAsync;
            Id = id;
            DisplayName = displayName;
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
                DisplayName
            );
        }


        /// <inheritdoc/>
        public override int GetHashCode() {
#if NETSTANDARD2_1 == false
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
                if (DisplayName != null) {
                    hash = (hash * 16777619) ^ DisplayName.GetHashCode();
                }
                return hash;
            }
#else
            return HashCode.Combine(Id, WorkItem, WorkItemAsync, DisplayName);
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
                string.Equals(other.DisplayName, DisplayName, StringComparison.Ordinal) && 
                other.WorkItem == WorkItem && 
                other.WorkItemAsync == WorkItemAsync;
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
