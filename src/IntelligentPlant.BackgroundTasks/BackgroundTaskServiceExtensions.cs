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

        #region [ Full Delegate with Activity ]

        /// <summary>
        /// Adds a synchronous work item to the queue.
        /// </summary>
        /// <param name="backgroundTaskService">
        ///   The <see cref="IBackgroundTaskService"/>.
        /// </param>
        /// <param name="workItem">
        ///   The work item.
        /// </param>
        /// <param name="activity">
        ///   The optional <see cref="Activity"/> to assign to the work item. The <see cref="Activity"/> 
        ///   will be disposed when the work item is completed.
        /// </param>
        /// <param name="cancellationToken">
        ///   An optional additional cancellation token for the work item.
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
        public static string QueueBackgroundWorkItem(this IBackgroundTaskService backgroundTaskService, Action<Activity?, CancellationToken> workItem, Activity? activity = null, CancellationToken cancellationToken = default) {
            if (backgroundTaskService == null) {
                throw new ArgumentNullException(nameof(backgroundTaskService));
            }
            if (workItem == null) {
                throw new ArgumentNullException(nameof(workItem));
            }

            if (cancellationToken != default) {
                return backgroundTaskService.QueueBackgroundWorkItem(workItem, activity, new[] { cancellationToken });
            }

            var item = new BackgroundWorkItem(workItem, activity);
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
        /// <param name="activity">
        ///   The optional <see cref="Activity"/> to assign to the work item. The <see cref="Activity"/> 
        ///   will be disposed when the work item is completed.
        /// </param>
        /// <param name="cancellationToken">
        ///   An optional additional cancellation token for the work item.
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
        public static string QueueBackgroundWorkItem(this IBackgroundTaskService backgroundTaskService, Func<Activity?, CancellationToken, Task> workItem, Activity? activity = null, CancellationToken cancellationToken = default) {
            if (backgroundTaskService == null) {
                throw new ArgumentNullException(nameof(backgroundTaskService));
            }
            if (workItem == null) {
                throw new ArgumentNullException(nameof(workItem));
            }

            if (cancellationToken != default) {
                return backgroundTaskService.QueueBackgroundWorkItem(workItem, activity, new[] { cancellationToken });
            }

            var item = new BackgroundWorkItem(workItem, activity);
            backgroundTaskService.QueueBackgroundWorkItem(item);

            return item.Id;
        }

        #endregion

        #region [ Simplified Delegate with Description and Activity ] 

        /// <summary>
        /// Adds a synchronous work item to the queue.
        /// </summary>
        /// <param name="backgroundTaskService">
        ///   The <see cref="IBackgroundTaskService"/>.
        /// </param>
        /// <param name="workItem">
        ///   The work item.
        /// </param>
        /// <param name="activity">
        ///   The optional <see cref="Activity"/> to assign to the work item. The <see cref="Activity"/> 
        ///   will be disposed when the work item is completed.
        /// </param>
        /// <param name="cancellationToken">
        ///   An optional additional cancellation token for the work item.
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
        public static string QueueBackgroundWorkItem(this IBackgroundTaskService backgroundTaskService, Action<CancellationToken> workItem, Activity? activity = null, CancellationToken cancellationToken = default) {
            if (backgroundTaskService == null) {
                throw new ArgumentNullException(nameof(backgroundTaskService));
            }
            if (workItem == null) {
                throw new ArgumentNullException(nameof(workItem));
            }

            if (cancellationToken != default) {
                return backgroundTaskService.QueueBackgroundWorkItem(workItem, activity, new[] { cancellationToken });
            }

            var item = new BackgroundWorkItem((a, ct) => workItem(ct), activity);
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
        /// <param name="activity">
        ///   The optional <see cref="Activity"/> to assign to the work item. The <see cref="Activity"/> 
        ///   will be disposed when the work item is completed.
        /// </param>
        /// <param name="cancellationToken">
        ///   An optional additional cancellation token for the work item.
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
        public static string QueueBackgroundWorkItem(this IBackgroundTaskService backgroundTaskService, Func<CancellationToken, Task> workItem, Activity? activity = null, CancellationToken cancellationToken = default) {
            if (backgroundTaskService == null) {
                throw new ArgumentNullException(nameof(backgroundTaskService));
            }
            if (workItem == null) {
                throw new ArgumentNullException(nameof(workItem));
            }

            if (cancellationToken != default) {
                return backgroundTaskService.QueueBackgroundWorkItem(workItem, activity, new[] { cancellationToken });
            }

            var item = new BackgroundWorkItem((a, ct) => workItem(ct), activity);
            backgroundTaskService.QueueBackgroundWorkItem(item);

            return item.Id;
        }

        #endregion

        #region [ Full Delegate with Description, Activity and Multiple Cancellation Tokens ]

        /// <summary>
        /// Adds a synchronous work item to the queue.
        /// </summary>
        /// <param name="backgroundTaskService">
        ///   The <see cref="IBackgroundTaskService"/>.
        /// </param>
        /// <param name="workItem">
        ///   The work item.
        /// </param>
        /// <param name="activity">
        ///   The optional <see cref="Activity"/> to assign to the work item. The <see cref="Activity"/> 
        ///   will be disposed when the work item is completed.
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
        public static string QueueBackgroundWorkItem(this IBackgroundTaskService backgroundTaskService, Action<Activity?, CancellationToken> workItem, Activity? activity, params CancellationToken[] tokens) {
            return QueueBackgroundWorkItem(backgroundTaskService, workItem, activity, (IEnumerable<CancellationToken>) tokens);
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
        /// <param name="activity">
        ///   The optional <see cref="Activity"/> to assign to the work item. The <see cref="Activity"/> 
        ///   will be disposed when the work item is completed.
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
        public static string QueueBackgroundWorkItem(this IBackgroundTaskService backgroundTaskService, Action<Activity?, CancellationToken> workItem, Activity? activity, IEnumerable<CancellationToken> tokens) {
            if (backgroundTaskService == null) {
                throw new ArgumentNullException(nameof(backgroundTaskService));
            }
            if (workItem == null) {
                throw new ArgumentNullException(nameof(workItem));
            }

            if (tokens == null || !tokens.Any()) {
                // No additional tokens; just queue the work item as normal.
                return backgroundTaskService.QueueBackgroundWorkItem(workItem, activity);
            }

            // We're constructing a new delegate to allow us to listen to multiple cancellation 
            // tokens. If no description has been specified, create a description now, using the 
            // original delegate provided to us, or the auto-generated description will reference 
            // this method instead of the original delegate.

            return backgroundTaskService.QueueBackgroundWorkItem((a, ct) => {
                using (var compositeTokenSource = CancellationTokenSource.CreateLinkedTokenSource(new[] { ct }.Concat(tokens).ToArray())) {
                    workItem(a, compositeTokenSource.Token);
                }
            }, activity);
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
        /// <param name="activity">
        ///   The optional <see cref="Activity"/> to assign to the work item. The <see cref="Activity"/> 
        ///   will be disposed when the work item is completed.
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
        public static string QueueBackgroundWorkItem(this IBackgroundTaskService backgroundTaskService, Func<Activity?, CancellationToken, Task> workItem, Activity? activity, params CancellationToken[] tokens) {
            return QueueBackgroundWorkItem(backgroundTaskService, workItem, activity, (IEnumerable<CancellationToken>) tokens);
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
        /// <param name="activity">
        ///   The optional <see cref="Activity"/> to assign to the work item. The <see cref="Activity"/> 
        ///   will be disposed when the work item is completed.
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
        public static string QueueBackgroundWorkItem(this IBackgroundTaskService backgroundTaskService, Func<Activity?, CancellationToken, Task> workItem, Activity? activity, IEnumerable<CancellationToken> tokens) {
            if (backgroundTaskService == null) {
                throw new ArgumentNullException(nameof(backgroundTaskService));
            }
            if (workItem == null) {
                throw new ArgumentNullException(nameof(workItem));
            }

            if (tokens == null || !tokens.Any()) {
                // No additional tokens; just queue the work item as normal.
                return backgroundTaskService.QueueBackgroundWorkItem(workItem, activity);
            }

            // We're constructing a new delegate to allow us to listen to multiple cancellation 
            // tokens. If no description has been specified, create a description now, using the 
            // original delegate provided to us, or the auto-generated description will reference 
            // this method instead of the original delegate.

            return backgroundTaskService.QueueBackgroundWorkItem(async (a, ct) => {
                using (var compositeTokenSource = CancellationTokenSource.CreateLinkedTokenSource(new[] { ct }.Concat(tokens).ToArray())) {
                    await workItem(a, compositeTokenSource.Token).ConfigureAwait(false);
                }
            }, activity);
        }

        #endregion

        #region [ Simplified Delegate with Description, Activity and Multiple Cancellation Tokens ]

        /// <summary>
        /// Adds a synchronous work item to the queue.
        /// </summary>
        /// <param name="backgroundTaskService">
        ///   The <see cref="IBackgroundTaskService"/>.
        /// </param>
        /// <param name="workItem">
        ///   The work item.
        /// </param>
        /// <param name="activity">
        ///   The optional <see cref="Activity"/> to assign to the work item. The <see cref="Activity"/> 
        ///   will be disposed when the work item is completed.
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
        public static string QueueBackgroundWorkItem(this IBackgroundTaskService backgroundTaskService, Action<CancellationToken> workItem, Activity? activity, params CancellationToken[] tokens) {
            return QueueBackgroundWorkItem(backgroundTaskService, workItem, activity, (IEnumerable<CancellationToken>) tokens);
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
        /// <param name="activity">
        ///   The optional <see cref="Activity"/> to assign to the work item. The <see cref="Activity"/> 
        ///   will be disposed when the work item is completed.
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
        public static string QueueBackgroundWorkItem(this IBackgroundTaskService backgroundTaskService, Action<CancellationToken> workItem, Activity? activity, IEnumerable<CancellationToken> tokens) {
            if (backgroundTaskService == null) {
                throw new ArgumentNullException(nameof(backgroundTaskService));
            }
            if (workItem == null) {
                throw new ArgumentNullException(nameof(workItem));
            }

            if (tokens == null || !tokens.Any()) {
                // No additional tokens; just queue the work item as normal.
                return backgroundTaskService.QueueBackgroundWorkItem(workItem, activity);
            }

            // We're constructing a new delegate to allow us to listen to multiple cancellation 
            // tokens. If no description has been specified, create a description now, using the 
            // original delegate provided to us, or the auto-generated description will reference 
            // this method instead of the original delegate.

            return backgroundTaskService.QueueBackgroundWorkItem((a, ct) => {
                using (var compositeTokenSource = CancellationTokenSource.CreateLinkedTokenSource(new[] { ct }.Concat(tokens).ToArray())) {
                    workItem(compositeTokenSource.Token);
                }
            }, activity);
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
        /// <param name="activity">
        ///   The optional <see cref="Activity"/> to assign to the work item. The <see cref="Activity"/> 
        ///   will be disposed when the work item is completed.
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
        public static string QueueBackgroundWorkItem(this IBackgroundTaskService backgroundTaskService, Func<CancellationToken, Task> workItem, Activity? activity, params CancellationToken[] tokens) {
            return QueueBackgroundWorkItem(backgroundTaskService, workItem, activity, (IEnumerable<CancellationToken>) tokens);
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
        /// <param name="activity">
        ///   The optional <see cref="Activity"/> to assign to the work item. The <see cref="Activity"/> 
        ///   will be disposed when the work item is completed.
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
        public static string QueueBackgroundWorkItem(this IBackgroundTaskService backgroundTaskService, Func<CancellationToken, Task> workItem, Activity? activity, IEnumerable<CancellationToken> tokens) {
            if (backgroundTaskService == null) {
                throw new ArgumentNullException(nameof(backgroundTaskService));
            }
            if (workItem == null) {
                throw new ArgumentNullException(nameof(workItem));
            }

            if (tokens == null || !tokens.Any()) {
                // No additional tokens; just queue the work item as normal.
                return backgroundTaskService.QueueBackgroundWorkItem(workItem, activity);
            }

            // We're constructing a new delegate to allow us to listen to multiple cancellation 
            // tokens. If no description has been specified, create a description now, using the 
            // original delegate provided to us, or the auto-generated description will reference 
            // this method instead of the original delegate.

            return backgroundTaskService.QueueBackgroundWorkItem(async (a, ct) => {
                using (var compositeTokenSource = CancellationTokenSource.CreateLinkedTokenSource(new[] { ct }.Concat(tokens).ToArray())) {
                    await workItem(compositeTokenSource.Token).ConfigureAwait(false);
                }
            }, activity);
        }

        #endregion

    }
}
