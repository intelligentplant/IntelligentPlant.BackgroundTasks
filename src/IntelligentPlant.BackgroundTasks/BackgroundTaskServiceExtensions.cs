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
        /// <param name="tokens">
        ///   Additional cancellation tokens for the operation. A composite token consisting of 
        ///   these tokens and the lifetime token of the <see cref="IBackgroundTaskService"/> will 
        ///   be passed to <paramref name="workItem"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="scheduler"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="workItem"/> is <see langword="null"/>.
        /// </exception>
        public static void QueueBackgroundWorkItem(this IBackgroundTaskService scheduler, Action<CancellationToken> workItem, params CancellationToken[] tokens) {
            QueueBackgroundWorkItem(scheduler, workItem, (IEnumerable<CancellationToken>) tokens);
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
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="scheduler"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="workItem"/> is <see langword="null"/>.
        /// </exception>
        public static void QueueBackgroundWorkItem(this IBackgroundTaskService scheduler, Action<CancellationToken> workItem, IEnumerable<CancellationToken> tokens) {
            if (scheduler == null) {
                throw new ArgumentNullException(nameof(scheduler));
            }
            if (workItem == null) {
                throw new ArgumentNullException(nameof(workItem));
            }

            if (tokens == null || !tokens.Any()) {
                // No additional tokens; just queue the work item as normal.
                scheduler.QueueBackgroundWorkItem(workItem);
                return;
            }

            scheduler.QueueBackgroundWorkItem(ct => {
                using (var compositeTokenSource = CancellationTokenSource.CreateLinkedTokenSource(new[] { ct }.Concat(tokens).ToArray())) {
                    workItem(compositeTokenSource.Token);
                }
            });
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
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="scheduler"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="workItem"/> is <see langword="null"/>.
        /// </exception>
        public static void QueueBackgroundWorkItem(this IBackgroundTaskService scheduler, Func<CancellationToken, Task> workItem, params CancellationToken[] tokens) {
            QueueBackgroundWorkItem(scheduler, workItem, (IEnumerable<CancellationToken>) tokens);
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
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="scheduler"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="workItem"/> is <see langword="null"/>.
        /// </exception>
        public static void QueueBackgroundWorkItem(this IBackgroundTaskService scheduler, Func<CancellationToken, Task> workItem, IEnumerable<CancellationToken> tokens) {
            if (scheduler == null) {
                throw new ArgumentNullException(nameof(scheduler));
            }
            if (workItem == null) {
                throw new ArgumentNullException(nameof(workItem));
            }

            if (tokens == null || !tokens.Any()) {
                // No additional tokens; just queue the work item as normal.
                scheduler.QueueBackgroundWorkItem(workItem);
                return;
            }

            scheduler.QueueBackgroundWorkItem(async ct => {
                using (var compositeTokenSource = CancellationTokenSource.CreateLinkedTokenSource(new[] { ct }.Concat(tokens).ToArray())) {
                    await workItem(compositeTokenSource.Token).ConfigureAwait(false);
                }
            });
        }

    }
}
