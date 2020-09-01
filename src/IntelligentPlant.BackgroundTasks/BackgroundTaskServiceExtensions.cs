using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IntelligentPlant.BackgroundTasks {

    /// <summary>
    /// Extensions for <see cref="IBackgroundTaskService"/>.
    /// </summary>
    public static class BackgroundTaskServiceExtensions {

        /// <summary>
        /// Adds a synchronous work item to the queue.
        /// </summary>
        /// <param name="scheduler">
        ///   The <see cref="IBackgroundTaskService"/>.
        /// </param>
        /// <param name="workItem">
        ///   The work item.
        /// </param>
        /// <param name="description">
        ///   The optional description for the work item.
        /// </param>
        /// <returns>
        ///   The unique identifier for the queued work item.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="scheduler"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="workItem"/> is <see langword="null"/>.
        /// </exception>
        public static Guid QueueBackgroundWorkItem(this IBackgroundTaskService scheduler, Action<CancellationToken> workItem, string? description = null) {
            if (scheduler == null) {
                throw new ArgumentNullException(nameof(scheduler));
            }
            if (workItem == null) {
                throw new ArgumentNullException(nameof(workItem));
            }

            var item = new BackgroundWorkItem(workItem, description);
            scheduler.QueueBackgroundWorkItem(item);

            return item.Id;
        }


        /// <summary>
        /// Adds an asynchronous work item, to the queue.
        /// </summary>
        /// <param name="scheduler">
        ///   The <see cref="IBackgroundTaskService"/>.
        /// </param>
        /// <param name="workItem">
        ///   The work item.
        /// </param>
        /// <param name="description">
        ///   The optional description for the work item.
        /// </param>
        /// <returns>
        ///   The unique identifier for the queued work item.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="scheduler"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="workItem"/> is <see langword="null"/>.
        /// </exception>
        public static Guid QueueBackgroundWorkItem(this IBackgroundTaskService scheduler, Func<CancellationToken, Task> workItem, string? description = null) {
            if (scheduler == null) {
                throw new ArgumentNullException(nameof(scheduler));
            }
            if (workItem == null) {
                throw new ArgumentNullException(nameof(workItem));
            }

            var item = new BackgroundWorkItem(workItem, description);
            scheduler.QueueBackgroundWorkItem(item);

            return item.Id;
        }


        /// <summary>
        /// Adds a synchronous work item to the queue.
        /// </summary>
        /// <param name="scheduler">
        ///   The <see cref="IBackgroundTaskService"/>.
        /// </param>
        /// <param name="workItem">
        ///   The work item.
        /// </param>
        /// <param name="tokens">
        ///   Additional cancellation tokens for the operation. A composite token consisting of 
        ///   these tokens and the lifetime token of the <see cref="IBackgroundTaskService"/> will 
        ///   be passed to <paramref name="workItem"/>.
        /// </param>
        /// <returns>
        ///   The unique identifier for the queued work item.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="scheduler"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="workItem"/> is <see langword="null"/>.
        /// </exception>
        public static Guid QueueBackgroundWorkItem(this IBackgroundTaskService scheduler, Action<CancellationToken> workItem, params CancellationToken[] tokens) {
            return QueueBackgroundWorkItem(scheduler, workItem, null, (IEnumerable<CancellationToken>) tokens);
        }


        /// <summary>
        /// Adds a synchronous work item to the queue.
        /// </summary>
        /// <param name="scheduler">
        ///   The <see cref="IBackgroundTaskService"/>.
        /// </param>
        /// <param name="workItem">
        ///   The work item.
        /// </param>
        /// <param name="description">
        ///   The description for the work item. Can be <see langword="null"/>.
        /// </param>
        /// <param name="tokens">
        ///   Additional cancellation tokens for the operation. A composite token consisting of 
        ///   these tokens and the lifetime token of the <see cref="IBackgroundTaskService"/> will 
        ///   be passed to <paramref name="workItem"/>.
        /// </param>
        /// <returns>
        ///   The unique identifier for the queued work item.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="scheduler"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="workItem"/> is <see langword="null"/>.
        /// </exception>
        public static Guid QueueBackgroundWorkItem(this IBackgroundTaskService scheduler, Action<CancellationToken> workItem, string? description, params CancellationToken[] tokens) {
            return QueueBackgroundWorkItem(scheduler, workItem, description, (IEnumerable<CancellationToken>) tokens);
        }


        /// <summary>
        /// Adds a synchronous work item to the queue.
        /// </summary>
        /// <param name="scheduler">
        ///   The <see cref="IBackgroundTaskService"/>.
        /// </param>
        /// <param name="workItem">
        ///   The work item.
        /// </param>
        /// <param name="description">
        ///   The description for the work item. Can be <see langword="null"/>.
        /// </param>
        /// <param name="tokens">
        ///   Additional cancellation tokens for the operation. A composite token consisting of 
        ///   these tokens and the lifetime token of the <see cref="IBackgroundTaskService"/> will 
        ///   be passed to <paramref name="workItem"/>.
        /// </param>
        /// <returns>
        ///   The unique identifier for the queued work item.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="scheduler"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="workItem"/> is <see langword="null"/>.
        /// </exception>
        public static Guid QueueBackgroundWorkItem(this IBackgroundTaskService scheduler, Action<CancellationToken> workItem, string? description, IEnumerable<CancellationToken> tokens) {
            if (scheduler == null) {
                throw new ArgumentNullException(nameof(scheduler));
            }
            if (workItem == null) {
                throw new ArgumentNullException(nameof(workItem));
            }

            if (tokens == null || !tokens.Any()) {
                // No additional tokens; just queue the work item as normal.
                return scheduler.QueueBackgroundWorkItem(workItem, description);
            }

            // We're constructing a new delegate to allow us to listen to multiple cancellation 
            // tokens. If no description has been specified, create a description now, using the 
            // original delegate provided to us, or the auto-generated description will reference 
            // this method instead of the original delegate.

            if (string.IsNullOrWhiteSpace(description)) {
                description = BackgroundWorkItem.CreateDescriptionFromDelegate(workItem);
            }

            return scheduler.QueueBackgroundWorkItem(ct => {
                using (var compositeTokenSource = CancellationTokenSource.CreateLinkedTokenSource(new[] { ct }.Concat(tokens).ToArray())) {
                    workItem(compositeTokenSource.Token);
                }
            }, description);
        }


        /// <summary>
        /// Adds a synchronous work item to the queue.
        /// </summary>
        /// <param name="scheduler">
        ///   The <see cref="IBackgroundTaskService"/>.
        /// </param>
        /// <param name="workItem">
        ///   The work item.
        /// </param>
        /// <param name="tokens">
        ///   Additional cancellation tokens for the operation. A composite token consisting of 
        ///   these tokens and the lifetime token of the <see cref="IBackgroundTaskService"/> will 
        ///   be passed to <paramref name="workItem"/>.
        /// </param>
        /// <returns>
        ///   The unique identifier for the queued work item.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="scheduler"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="workItem"/> is <see langword="null"/>.
        /// </exception>
        public static Guid QueueBackgroundWorkItem(this IBackgroundTaskService scheduler, Func<CancellationToken, Task> workItem, params CancellationToken[] tokens) {
            return QueueBackgroundWorkItem(scheduler, workItem, null, (IEnumerable<CancellationToken>) tokens);
        }


        /// <summary>
        /// Adds a synchronous work item to the queue.
        /// </summary>
        /// <param name="scheduler">
        ///   The <see cref="IBackgroundTaskService"/>.
        /// </param>
        /// <param name="workItem">
        ///   The work item.
        /// </param>
        /// <param name="description">
        ///   The description for the work item. Can be <see langword="null"/>.
        /// </param>
        /// <param name="tokens">
        ///   Additional cancellation tokens for the operation. A composite token consisting of 
        ///   these tokens and the lifetime token of the <see cref="IBackgroundTaskService"/> will 
        ///   be passed to <paramref name="workItem"/>.
        /// </param>
        /// <returns>
        ///   The unique identifier for the queued work item.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="scheduler"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="workItem"/> is <see langword="null"/>.
        /// </exception>
        public static Guid QueueBackgroundWorkItem(this IBackgroundTaskService scheduler, Func<CancellationToken, Task> workItem, string? description, params CancellationToken[] tokens) {
            return QueueBackgroundWorkItem(scheduler, workItem, description, (IEnumerable<CancellationToken>) tokens);
        }


        /// <summary>
        /// Adds a synchronous work item to the queue.
        /// </summary>
        /// <param name="scheduler">
        ///   The <see cref="IBackgroundTaskService"/>.
        /// </param>
        /// <param name="workItem">
        ///   The work item.
        /// </param>
        /// <param name="description">
        ///   The description for the work item. Can be <see langword="null"/>.
        /// </param>
        /// <param name="tokens">
        ///   Additional cancellation tokens for the operation. A composite token consisting of 
        ///   these tokens and the lifetime token of the <see cref="IBackgroundTaskService"/> will 
        ///   be passed to <paramref name="workItem"/>.
        /// </param>
        /// <returns>
        ///   The unique identifier for the queued work item.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="scheduler"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="workItem"/> is <see langword="null"/>.
        /// </exception>
        public static Guid QueueBackgroundWorkItem(this IBackgroundTaskService scheduler, Func<CancellationToken, Task> workItem, string? description, IEnumerable<CancellationToken> tokens) {
            if (scheduler == null) {
                throw new ArgumentNullException(nameof(scheduler));
            }
            if (workItem == null) {
                throw new ArgumentNullException(nameof(workItem));
            }

            if (tokens == null || !tokens.Any()) {
                // No additional tokens; just queue the work item as normal.
                return scheduler.QueueBackgroundWorkItem(workItem, description);
            }

            // We're constructing a new delegate to allow us to listen to multiple cancellation 
            // tokens. If no description has been specified, create a description now, using the 
            // original delegate provided to us, or the auto-generated description will reference 
            // this method instead of the original delegate.

            if (string.IsNullOrWhiteSpace(description)) {
                description = BackgroundWorkItem.CreateDescriptionFromDelegate(workItem);
            }

            return scheduler.QueueBackgroundWorkItem(async ct => {
                using (var compositeTokenSource = CancellationTokenSource.CreateLinkedTokenSource(new[] { ct }.Concat(tokens).ToArray())) {
                    await workItem(compositeTokenSource.Token).ConfigureAwait(false);
                }
            }, description);
        }

    }
}
