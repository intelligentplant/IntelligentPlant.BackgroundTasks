using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        /// <param name="backgroundTaskService">
        ///   The <see cref="IBackgroundTaskService"/>.
        /// </param>
        /// <param name="workItem">
        ///   The work item.
        /// </param>
        /// <param name="displayName">
        ///   The display name for the work item.
        /// </param>
        /// <param name="captureParentActivity">
        ///   When <see langword="true"/>, the value of <see cref="Activity.Current"/> at the 
        ///   moment that the <see cref="BackgroundWorkItem"/> is created will be restored onto 
        ///   the work item thread immediately before the item is run.
        /// </param>
        /// <returns>
        ///   The unique identifier for the queued work item.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="backgroundTaskService"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="workItem"/> is <see langword="null"/>.
        /// </exception>
        public static string QueueBackgroundWorkItem(
            this IBackgroundTaskService backgroundTaskService,
            Action<CancellationToken> workItem,
            string? displayName = null,
            bool captureParentActivity = false
        ) {
            if (backgroundTaskService == null) {
                throw new ArgumentNullException(nameof(backgroundTaskService));
            }
            if (workItem == null) {
                throw new ArgumentNullException(nameof(workItem));
            }

            var item = new BackgroundWorkItem(workItem, displayName, captureParentActivity);
            backgroundTaskService.QueueBackgroundWorkItem(item);

            return item.Id;
        }


        /// <summary>
        /// Adds an asynchronous work item to the queue.
        /// </summary>
        /// <param name="backgroundTaskService">
        ///   The <see cref="IBackgroundTaskService"/>.
        /// </param>
        /// <param name="workItem">
        ///   The work item.
        /// </param>
        /// <param name="displayName">
        ///   The display name for the work item.
        /// </param>
        /// <param name="captureParentActivity">
        ///   When <see langword="true"/>, the value of <see cref="Activity.Current"/> at the 
        ///   moment that the <see cref="BackgroundWorkItem"/> is created will be restored onto 
        ///   the work item thread immediately before the item is run.
        /// </param>
        /// <returns>
        ///   The unique identifier for the queued work item.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="backgroundTaskService"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="workItem"/> is <see langword="null"/>.
        /// </exception>
        public static string QueueBackgroundWorkItem(
            this IBackgroundTaskService backgroundTaskService,
            Func<CancellationToken, Task> workItem,
            string? displayName = null,
            bool captureParentActivity = false
        ) {
            if (backgroundTaskService == null) {
                throw new ArgumentNullException(nameof(backgroundTaskService));
            }
            if (workItem == null) {
                throw new ArgumentNullException(nameof(workItem));
            }

            var item = new BackgroundWorkItem(workItem, displayName, captureParentActivity);
            backgroundTaskService.QueueBackgroundWorkItem(item);

            return item.Id;
        }


        /// <summary>
        /// Adds a synchronous work item to the queue.
        /// </summary>
        /// <param name="backgroundTaskService">
        ///   The <see cref="IBackgroundTaskService"/>.
        /// </param>
        /// <param name="workItem">
        ///   The work item.
        /// </param>
        /// <param name="displayName">
        ///   The display name for the work item.
        /// </param>
        /// <param name="captureParentActivity">
        ///   When <see langword="true"/>, the value of <see cref="Activity.Current"/> at the 
        ///   moment that the <see cref="BackgroundWorkItem"/> is created will be restored onto 
        ///   the work item thread immediately before the item is run.
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
        ///   <paramref name="backgroundTaskService"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="workItem"/> is <see langword="null"/>.
        /// </exception>
        public static string QueueBackgroundWorkItem(
            this IBackgroundTaskService backgroundTaskService,
            Action<CancellationToken> workItem,
            string? displayName,
            bool captureParentActivity,
            params CancellationToken[] tokens
        ) {
            return QueueBackgroundWorkItem(backgroundTaskService, workItem, displayName, captureParentActivity, (IEnumerable<CancellationToken>) tokens);
        }


        /// <summary>
        /// Adds a synchronous work item to the queue.
        /// </summary>
        /// <param name="backgroundTaskService">
        ///   The <see cref="IBackgroundTaskService"/>.
        /// </param>
        /// <param name="workItem">
        ///   The work item.
        /// </param>
        /// <param name="displayName">
        ///   The display name for the work item.
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
        ///   <paramref name="backgroundTaskService"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="workItem"/> is <see langword="null"/>.
        /// </exception>
        public static string QueueBackgroundWorkItem(
            this IBackgroundTaskService backgroundTaskService,
            Action<CancellationToken> workItem,
            string? displayName,
            params CancellationToken[] tokens
        ) {
            return QueueBackgroundWorkItem(backgroundTaskService, workItem, displayName, false, (IEnumerable<CancellationToken>) tokens);
        }


        /// <summary>
        /// Adds a synchronous work item to the queue.
        /// </summary>
        /// <param name="backgroundTaskService">
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
        ///   <paramref name="backgroundTaskService"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="workItem"/> is <see langword="null"/>.
        /// </exception>
        public static string QueueBackgroundWorkItem(
            this IBackgroundTaskService backgroundTaskService,
            Action<CancellationToken> workItem,
            params CancellationToken[] tokens
        ) {
            return QueueBackgroundWorkItem(backgroundTaskService, workItem, null, false, (IEnumerable<CancellationToken>) tokens);
        }


        /// <summary>
        /// Adds a synchronous work item to the queue.
        /// </summary>
        /// <param name="backgroundTaskService">
        ///   The <see cref="IBackgroundTaskService"/>.
        /// </param>
        /// <param name="workItem">
        ///   The work item.
        /// </param>
        /// <param name="displayName">
        ///   The display name for the work item.
        /// </param>
        /// <param name="captureParentActivity">
        ///   When <see langword="true"/>, the value of <see cref="Activity.Current"/> at the 
        ///   moment that the <see cref="BackgroundWorkItem"/> is created will be restored onto 
        ///   the work item thread immediately before the item is run.
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
        ///   <paramref name="backgroundTaskService"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="workItem"/> is <see langword="null"/>.
        /// </exception>
        public static string QueueBackgroundWorkItem(
            this IBackgroundTaskService backgroundTaskService,
            Action<CancellationToken> workItem,
            string? displayName,
            bool captureParentActivity,
            IEnumerable<CancellationToken>? tokens
        ) {
            if (backgroundTaskService == null) {
                throw new ArgumentNullException(nameof(backgroundTaskService));
            }
            if (workItem == null) {
                throw new ArgumentNullException(nameof(workItem));
            }

            var additionalTokens = tokens?.ToArray();

            if (additionalTokens?.Length == 0) {
                // No additional tokens; just queue the work item as normal.
                return backgroundTaskService.QueueBackgroundWorkItem(workItem, displayName, captureParentActivity);
            }

            // We're constructing a new delegate to allow us to listen to multiple cancellation 
            // tokens.

            return backgroundTaskService.QueueBackgroundWorkItem(ct => {
                using (var compositeTokenSource = CancellationTokenSource.CreateLinkedTokenSource(new[] { ct }.Concat(additionalTokens).ToArray())) {
                    workItem(compositeTokenSource.Token);
                }
            }, displayName, captureParentActivity);
        }


        /// <summary>
        /// Adds an asynchronous work item to the queue.
        /// </summary>
        /// <param name="backgroundTaskService">
        ///   The <see cref="IBackgroundTaskService"/>.
        /// </param>
        /// <param name="workItem">
        ///   The work item.
        /// </param>
        /// <param name="displayName">
        ///   The display name for the work item.
        /// </param>
        /// <param name="captureParentActivity">
        ///   When <see langword="true"/>, the value of <see cref="Activity.Current"/> at the 
        ///   moment that the <see cref="BackgroundWorkItem"/> is created will be restored onto 
        ///   the work item thread immediately before the item is run.
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
        ///   <paramref name="backgroundTaskService"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="workItem"/> is <see langword="null"/>.
        /// </exception>
        public static string QueueBackgroundWorkItem(
            this IBackgroundTaskService backgroundTaskService,
            Func<CancellationToken, Task> workItem,
            string? displayName,
            bool captureParentActivity,
            params CancellationToken[] tokens
        ) {
            return QueueBackgroundWorkItem(backgroundTaskService, workItem, displayName, captureParentActivity, (IEnumerable<CancellationToken>) tokens);
        }


        /// <summary>
        /// Adds an asynchronous work item to the queue.
        /// </summary>
        /// <param name="backgroundTaskService">
        ///   The <see cref="IBackgroundTaskService"/>.
        /// </param>
        /// <param name="workItem">
        ///   The work item.
        /// </param>
        /// <param name="displayName">
        ///   The display name for the work item.
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
        ///   <paramref name="backgroundTaskService"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="workItem"/> is <see langword="null"/>.
        /// </exception>
        public static string QueueBackgroundWorkItem(
            this IBackgroundTaskService backgroundTaskService,
            Func<CancellationToken, Task> workItem,
            string? displayName,
            params CancellationToken[] tokens
        ) {
            return QueueBackgroundWorkItem(backgroundTaskService, workItem, displayName, false, (IEnumerable<CancellationToken>) tokens);
        }


        /// <summary>
        /// Adds an asynchronous work item to the queue.
        /// </summary>
        /// <param name="backgroundTaskService">
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
        ///   <paramref name="backgroundTaskService"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="workItem"/> is <see langword="null"/>.
        /// </exception>
        public static string QueueBackgroundWorkItem(
            this IBackgroundTaskService backgroundTaskService,
            Func<CancellationToken, Task> workItem,
            params CancellationToken[] tokens
        ) {
            return QueueBackgroundWorkItem(backgroundTaskService, workItem, null, false, (IEnumerable<CancellationToken>) tokens);
        }


        /// <summary>
        /// Adds an asynchronous work item to the queue.
        /// </summary>
        /// <param name="backgroundTaskService">
        ///   The <see cref="IBackgroundTaskService"/>.
        /// </param>
        /// <param name="workItem">
        ///   The work item.
        /// </param>
        /// <param name="captureParentActivity">
        ///   When <see langword="true"/>, the value of <see cref="Activity.Current"/> at the 
        ///   moment that the <see cref="BackgroundWorkItem"/> is created will be restored onto 
        ///   the work item thread immediately before the item is run.
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
        ///   <paramref name="backgroundTaskService"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="workItem"/> is <see langword="null"/>.
        /// </exception>
        public static string QueueBackgroundWorkItem(
            this IBackgroundTaskService backgroundTaskService,
            Func<CancellationToken, Task> workItem,
            string? displayName,
            bool captureParentActivity,
            IEnumerable<CancellationToken>? tokens
        ) {
            if (backgroundTaskService == null) {
                throw new ArgumentNullException(nameof(backgroundTaskService));
            }
            if (workItem == null) {
                throw new ArgumentNullException(nameof(workItem));
            }

            var additionalTokens = tokens?.ToArray();

            if (additionalTokens?.Length == 0) {
                // No additional tokens; just queue the work item as normal.
                return backgroundTaskService.QueueBackgroundWorkItem(workItem, displayName, captureParentActivity);
            }

            // We're constructing a new delegate to allow us to listen to multiple cancellation 
            // tokens. If no description has been specified, create a description now, using the 
            // original delegate provided to us, or the auto-generated description will reference 
            // this method instead of the original delegate.

            return backgroundTaskService.QueueBackgroundWorkItem(async (ct) => {
                using (var compositeTokenSource = CancellationTokenSource.CreateLinkedTokenSource(new[] { ct }.Concat(additionalTokens).ToArray())) {
                    await workItem(compositeTokenSource.Token).ConfigureAwait(false);
                }
            }, displayName, captureParentActivity);
        }

    }
}
