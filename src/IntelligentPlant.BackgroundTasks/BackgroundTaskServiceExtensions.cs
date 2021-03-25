﻿using System;
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
        /// <param name="activityFactory">
        ///   A factory function that can be used to start an <see cref="Activity"/> associated 
        ///   with the work item when it is run.
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
            Func<Activity?>? activityFactory = null
        ) {
            if (backgroundTaskService == null) {
                throw new ArgumentNullException(nameof(backgroundTaskService));
            }
            if (workItem == null) {
                throw new ArgumentNullException(nameof(workItem));
            }

            var item = new BackgroundWorkItem(workItem, displayName, activityFactory);
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
        /// <param name="activityFactory">
        ///   A factory function that can be used to start an <see cref="Activity"/> associated 
        ///   with the work item when it is run.
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
            Func<Activity?>? activityFactory = null
        ) {
            if (backgroundTaskService == null) {
                throw new ArgumentNullException(nameof(backgroundTaskService));
            }
            if (workItem == null) {
                throw new ArgumentNullException(nameof(workItem));
            }

            var item = new BackgroundWorkItem(workItem, displayName, activityFactory);
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
        /// <param name="activityFactory">
        ///   A factory function that can be used to start an <see cref="Activity"/> associated 
        ///   with the work item when it is run.
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
            Func<Activity?>? activityFactory, 
            params CancellationToken[] tokens
        ) {
            return QueueBackgroundWorkItem(backgroundTaskService, workItem, displayName, activityFactory, (IEnumerable<CancellationToken>) tokens);
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
        /// <param name="activityFactory">
        ///   A factory function that can be used to start an <see cref="Activity"/> associated 
        ///   with the work item when it is run.
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
            Func<Activity?>? activityFactory, 
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
                return backgroundTaskService.QueueBackgroundWorkItem(workItem, displayName, activityFactory);
            }

            // We're constructing a new delegate to allow us to listen to multiple cancellation 
            // tokens.

            return backgroundTaskService.QueueBackgroundWorkItem(ct => {
                using (var compositeTokenSource = CancellationTokenSource.CreateLinkedTokenSource(new[] { ct }.Concat(additionalTokens).ToArray())) {
                    workItem(compositeTokenSource.Token);
                }
            }, displayName, activityFactory);
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
        /// <param name="activityFactory">
        ///   A factory function that can be used to start an <see cref="Activity"/> associated 
        ///   with the work item when it is run.
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
            Func<Activity?>? activityFactory,
            params CancellationToken[] tokens
        ) {
            return QueueBackgroundWorkItem(backgroundTaskService, workItem, displayName, activityFactory, (IEnumerable<CancellationToken>) tokens);
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
        public static string QueueBackgroundWorkItem(
            this IBackgroundTaskService backgroundTaskService, 
            Func<CancellationToken, Task> workItem,
            string? displayName,
            Func<Activity?>? activityFactory,
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
                return backgroundTaskService.QueueBackgroundWorkItem(workItem, displayName, activityFactory);
            }

            // We're constructing a new delegate to allow us to listen to multiple cancellation 
            // tokens. If no description has been specified, create a description now, using the 
            // original delegate provided to us, or the auto-generated description will reference 
            // this method instead of the original delegate.

            return backgroundTaskService.QueueBackgroundWorkItem(async (ct) => {
                using (var compositeTokenSource = CancellationTokenSource.CreateLinkedTokenSource(new[] { ct }.Concat(additionalTokens).ToArray())) {
                    await workItem(compositeTokenSource.Token).ConfigureAwait(false);
                }
            }, displayName, activityFactory);
        }

    }
}
