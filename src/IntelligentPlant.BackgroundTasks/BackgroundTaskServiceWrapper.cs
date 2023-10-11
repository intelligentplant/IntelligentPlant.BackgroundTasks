using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace IntelligentPlant.BackgroundTasks {

    /// <summary>
    /// <see cref="IBackgroundTaskService"/> that wraps an existing <see cref="IBackgroundTaskService"/> 
    /// instance but allows additional cancellation tokens to be applied to all work items that 
    /// are registered.
    /// </summary>
    public sealed class BackgroundTaskServiceWrapper : IBackgroundTaskService {

        /// <summary>
        /// Flags if the object has been disposed;
        /// </summary>
        private bool _isDisposed;

        /// <summary>
        /// The inner <see cref="IBackgroundTaskService"/> instance.
        /// </summary>
        private readonly IBackgroundTaskService _inner;

        /// <summary>
        /// The callback delegate that will return the additional cancellation tokens to observe 
        /// when registering a work item.
        /// </summary>
        private readonly Func<IEnumerable<CancellationToken>> _callback;

        /// <inheritdoc/>
        public bool IsRunning => _isDisposed ? false : _inner.IsRunning;

        /// <inheritdoc/>
        public int QueuedItemCount => _isDisposed ? 0 : _inner.QueuedItemCount;

        /// <inheritdoc/>
        public event EventHandler<BackgroundWorkItem>? BeforeWorkItemStarted {
            add {
                if (!_isDisposed) {
                    _inner.BeforeWorkItemStarted += value;
                }
            }
            remove {
                if (!_isDisposed) {
                    _inner.BeforeWorkItemStarted -= value;
                }
            }
        }

        /// <inheritdoc/>
        public event EventHandler<BackgroundWorkItem>? AfterWorkItemCompleted {
            add {
                if (!_isDisposed) {
                    _inner.AfterWorkItemCompleted += value;
                }
            }
            remove {
                if (!_isDisposed) {
                    _inner.AfterWorkItemCompleted -= value;
                }
            }
        }


        /// <summary>
        /// Creates a new <see cref="BackgroundTaskServiceWrapper"/> object.
        /// </summary>
        /// <param name="inner">
        ///   The inner <see cref="IBackgroundTaskService"/> object to wrap.
        /// </param>
        /// <param name="cancellationTokens">
        ///   The additional cancellation tokens to observe when registering new work items.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="inner"/> is <see langword="null"/>.
        /// </exception>
        public BackgroundTaskServiceWrapper(IBackgroundTaskService inner, params CancellationToken[] cancellationTokens)
            : this(inner, (IEnumerable<CancellationToken>) cancellationTokens){ }


        /// <summary>
        /// Creates a new <see cref="BackgroundTaskServiceWrapper"/> object.
        /// </summary>
        /// <param name="inner">
        ///   The inner <see cref="IBackgroundTaskService"/> object to wrap.
        /// </param>
        /// <param name="cancellationTokens">
        ///   The additional cancellation tokens to observe when registering new work items.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="inner"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="cancellationTokens"/> is <see langword="null"/>.
        /// </exception>
        public BackgroundTaskServiceWrapper(IBackgroundTaskService inner, IEnumerable<CancellationToken> cancellationTokens) {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            if (cancellationTokens == null) {
                throw new ArgumentNullException(nameof(cancellationTokens));
            }
            _callback = () => cancellationTokens;
        }


        /// <summary>
        /// Creates a new <see cref="BackgroundTaskServiceWrapper"/> object.
        /// </summary>
        /// <param name="inner">
        ///   The inner <see cref="IBackgroundTaskService"/> object to wrap.
        /// </param>
        /// <param name="callback">
        ///   The callback that will be used to retrieve an additional cancellation token to observe 
        ///   when registering new work items.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="inner"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="callback"/> is <see langword="null"/>.
        /// </exception>
        public BackgroundTaskServiceWrapper(IBackgroundTaskService inner, Func<CancellationToken> callback) {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            if (callback == null) {
                throw new ArgumentNullException(nameof(callback));
            }
            _callback = () => new[] { callback.Invoke() };
        }


        /// <summary>
        /// Creates a new <see cref="BackgroundTaskServiceWrapper"/> object.
        /// </summary>
        /// <param name="inner">
        ///   The inner <see cref="IBackgroundTaskService"/> object to wrap.
        /// </param>
        /// <param name="callback">
        ///   The callback that will be used to retrieve additional cancellation tokens to observe 
        ///   when registering new work items.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="inner"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="callback"/> is <see langword="null"/>.
        /// </exception>
        public BackgroundTaskServiceWrapper(IBackgroundTaskService inner, Func<IEnumerable<CancellationToken>> callback) {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _callback = callback ?? throw new ArgumentNullException(nameof(callback));
        }


        /// <inheritdoc/>
        public void QueueBackgroundWorkItem(BackgroundWorkItem workItem) {
            if (_isDisposed) {
                throw new ObjectDisposedException(GetType().FullName);
            }

            BackgroundWorkItem wrappedWorkItem;
            var tokens = _callback.Invoke();

            if (workItem.WorkItemAsync != null) {
                wrappedWorkItem = new BackgroundWorkItem(
                    null, 
                    async ct => {
                        using (var ctSource = CancellationTokenSource.CreateLinkedTokenSource(new[] { ct }.Concat(tokens).ToArray())) {
                            await workItem.WorkItemAsync.Invoke(ctSource.Token).ConfigureAwait(false);
                        }
                    }, 
                    workItem.Id, 
                    workItem.DisplayName,
                    workItem.ParentActivity
                );
            }
            else {
                wrappedWorkItem = new BackgroundWorkItem(
                    ct => {
                        using (var ctSource = CancellationTokenSource.CreateLinkedTokenSource(new[] { ct }.Concat(tokens).ToArray())) {
                            workItem.WorkItem!.Invoke(ctSource.Token);
                        }
                    },
                    null,
                    workItem.Id,
                    workItem.DisplayName,
                    workItem.ParentActivity
                );
            }

            _inner.QueueBackgroundWorkItem(wrappedWorkItem);
        }


        /// <inheritdoc/>
        public void Dispose() {
            _isDisposed = true;
            // No other action required.
        }

    }
}
