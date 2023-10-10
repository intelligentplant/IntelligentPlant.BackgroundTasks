using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace IntelligentPlant.BackgroundTasks {

    /// <summary>
    /// Describes a work item that has been added to an <see cref="IBackgroundTaskService"/> queue.
    /// </summary>
    public readonly struct BackgroundWorkItem : IEquatable<BackgroundWorkItem> {

        /// <summary>
        /// The parent activity for the background work item.
        /// </summary>
        internal Activity? ParentActivity { get; }

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
        /// A task completion source that is used to signal when the work item has finished.
        /// </summary>
        private readonly TaskCompletionSource<object?> _completionSource;

        /// <summary>
        /// A task that completes when the work item has finished executing.
        /// </summary>
        public Task Task => _completionSource.Task;


        /// <summary>
        /// Creates a new <see cref="BackgroundWorkItem"/> with a synchronous work item.
        /// </summary>
        /// <param name="workItem">
        ///   The work item.
        /// </param>
        /// <param name="displayName">
        ///   The display name for the work item.
        /// </param>
        /// <param name="captureParentActivity">
        ///   When <see langword="true"/>, the value of <see cref="Activity.Current"/> at the 
        ///   moment that the <see cref="BackgroundWorkItem"/> is created will be captured and set 
        ///   as the value of <see cref="Activity.Current"/> immediately before the background work 
        ///   item is run.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="workItem"/> is <see langword="null"/>.
        /// </exception>
        public BackgroundWorkItem(Action<CancellationToken> workItem, string? displayName = null, bool captureParentActivity = false) {
            WorkItem = workItem ?? throw new ArgumentNullException(nameof(workItem));
            WorkItemAsync = null;

            ParentActivity = captureParentActivity ? Activity.Current : null;
            Id = Guid.NewGuid().ToString();
            DisplayName = displayName;
            _completionSource = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
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
        /// <param name="captureParentActivity">
        ///   When <see langword="true"/>, the value of <see cref="Activity.Current"/> at the 
        ///   moment that the <see cref="BackgroundWorkItem"/> is created will be captured and set 
        ///   as the value of <see cref="Activity.Current"/> immediately before the background work 
        ///   item is run.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="workItem"/> is <see langword="null"/>.
        /// </exception>
        public BackgroundWorkItem(Func<CancellationToken, Task> workItem, string? displayName = null, bool captureParentActivity = false) {
            WorkItem = null;
            WorkItemAsync = workItem ?? throw new ArgumentNullException(nameof(workItem));

            ParentActivity = captureParentActivity ? Activity.Current : null;
            Id = Guid.NewGuid().ToString();
            DisplayName = displayName;
            _completionSource = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
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
        /// <param name="parentActivity">
        ///   The parent activity for the work item.
        /// </param>
        internal BackgroundWorkItem(
            Action<CancellationToken>? workItem,
            Func<CancellationToken, Task>? workItemAsync,
            string id,
            string? displayName,
            Activity? parentActivity
        ) {
            WorkItem = workItem;
            WorkItemAsync = workItemAsync;

            ParentActivity = parentActivity;
            Id = id;
            DisplayName = displayName;
            _completionSource = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
        }


        /// <summary>
        /// Marks the work item as completed.
        /// </summary>
        /// <param name="error">
        ///   When non-<see langword="null"/>, the work item is marked as faulted with this exception.
        /// </param>
        internal void OnCompleted(Exception? error = null) {
            if (error == null) {
                _completionSource.TrySetResult(null);
            }
            else if (error is OperationCanceledException cancelled) {
                if (cancelled.CancellationToken == default) {
                    _completionSource.TrySetCanceled();
                }
                else {
                    _completionSource.TrySetCanceled(cancelled.CancellationToken);
                }
            }
            else {
                _completionSource.TrySetException(error);
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
                DisplayName,
                _completionSource.Task.Status switch {
                    TaskStatus.Canceled => "Cancelled",
                    TaskStatus.Faulted => "Faulted",
                    TaskStatus.RanToCompletion => "Completed",
                    _ => "Pending"
                }
            );
        }


        /// <inheritdoc/>
        public override int GetHashCode() {
            return HashCode.Combine(Id, WorkItem, WorkItemAsync, DisplayName);
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
