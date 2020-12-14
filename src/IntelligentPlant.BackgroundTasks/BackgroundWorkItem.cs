using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace IntelligentPlant.BackgroundTasks {

    /// <summary>
    /// Describes a work item that has been added to a <see cref="BackgroundTaskService"/> queue.
    /// </summary>
    public struct BackgroundWorkItem : IEquatable<BackgroundWorkItem> {

        /// <summary>
        /// Gets the unique identifier for the work item.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Gets the optional description for the work item.
        /// </summary>
        public string? Description { get; }

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
        /// <param name="description">
        ///   The optional description for the work item. if no description is specified, a 
        ///   description will be constructed using the <see cref="MethodInfo"/> for the 
        ///   <paramref name="workItem"/>
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="workItem"/> is <see langword="null"/>.
        /// </exception>
        public BackgroundWorkItem(Action<CancellationToken> workItem, string? description = null) {
            Id = Guid.NewGuid();
            WorkItem = workItem ?? throw new ArgumentNullException(nameof(workItem));
            WorkItemAsync = null;

            if (string.IsNullOrWhiteSpace(description)) {
                description = CreateDescriptionFromDelegate(workItem);
            }

            Description = description;
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
        public BackgroundWorkItem(Func<CancellationToken, Task> workItem, string? description = null) {
            Id = Guid.NewGuid();
            WorkItem = null;
            WorkItemAsync = workItem ?? throw new ArgumentNullException(nameof(workItem));

            if (string.IsNullOrWhiteSpace(description)) {
                description = CreateDescriptionFromDelegate(workItem);
            }

            Description = description;
        }


        /// <summary>
        /// Creates a description for a <see cref="BackgroundWorkItem"/> using the 
        /// <see cref="MethodInfo"/> associated with the specified delegate.
        /// </summary>
        /// <param name="workItem">
        ///   The delegate.
        /// </param>
        /// <returns>
        ///   A description that can be used when creating a new <see cref="BackgroundWorkItem"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="workItem"/> is <see langword="null"/>.
        /// </exception>
        public static string CreateDescriptionFromDelegate(Action<CancellationToken> workItem) {
            if (workItem == null) {
                throw new ArgumentNullException(nameof(workItem));
            }

            var methodInfo = workItem.GetMethodInfo();
            return string.Concat(methodInfo.ReflectedType.FullName, ".", methodInfo.Name);
        }


        /// <summary>
        /// Creates a description for a <see cref="BackgroundWorkItem"/> using the 
        /// <see cref="MethodInfo"/> associated with the specified delegate.
        /// </summary>
        /// <param name="workItem">
        ///   The delegate.
        /// </param>
        /// <returns>
        ///   A description that can be used when creating a new <see cref="BackgroundWorkItem"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="workItem"/> is <see langword="null"/>.
        /// </exception>
        public static string CreateDescriptionFromDelegate(Func<CancellationToken, Task> workItem) {
            if (workItem == null) {
                throw new ArgumentNullException(nameof(workItem));
            }

            var methodInfo = workItem.GetMethodInfo();
            return string.Concat(methodInfo.ReflectedType.FullName, ".", methodInfo.Name);
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
            return other.Id.Equals(Id) && 
                string.Equals(other.Description, Description, StringComparison.Ordinal) && 
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
