using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace IntelligentPlant.BackgroundTasks {

    /// <summary>
    /// Base <see cref="IBackgroundTaskService"/> implementation. Call the <see cref="RunAsync"/> 
    /// method to start the service.
    /// </summary>
    public abstract class BackgroundTaskService : IBackgroundTaskService, IDisposable {

        /// <summary>
        /// The default background task service.
        /// </summary>
        public static IBackgroundTaskService Default { get; } = new DefaultBackgroundTaskService(null);

        /// <summary>
        /// The service options.
        /// </summary>
        private readonly BackgroundTaskServiceOptions _options;

        /// <summary>
        /// Flags if the service is running.
        /// </summary>
        private int _isRunning;

        /// <summary>
        /// Flags if the service has been disposed.
        /// </summary>
        private bool _isDisposed;

        /// <summary>
        /// The currently-queued work items.
        /// </summary>
        private readonly ConcurrentQueue<BackgroundWorkItem> _queue = new ConcurrentQueue<BackgroundWorkItem>();

        /// <summary>
        /// Signals when an item is added to the <see cref="_queue"/>.
        /// </summary>
        private readonly SemaphoreSlim _queueSignal = new SemaphoreSlim(0);

        /// <summary>
        /// Gets a flag indicating if the service is currently running.
        /// </summary>
        public bool IsRunning { get { return _isRunning != 0; } }

        /// <summary>
        /// Gets the number of work items that are currently queued.
        /// </summary>
        public int QueuedItemCount { get { return _queue.Count; } }


        /// <summary>
        /// Creates a new <see cref="BackgroundTaskService"/> object.
        /// </summary>
        /// <param name="options">
        ///   The options for the service..
        /// </param>
        protected BackgroundTaskService(BackgroundTaskServiceOptions options = null) {
            _options = options ?? new BackgroundTaskServiceOptions();
        }


        /// <summary>
        /// Throws an <see cref="ObjectDisposedException"/> if the task service has been disposed.
        /// </summary>
        protected void ThrowIfDisposed() {
            if (_isDisposed) {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }


        /// <inheritdoc/>
        public void QueueBackgroundWorkItem(BackgroundWorkItem workItem) {
            ThrowIfDisposed();

            if (workItem.WorkItem == null && workItem.WorkItemAsync == null) {
                // Default value; we'll ignore the item.
            }

            _queue.Enqueue(workItem);
            OnQueued(workItem);
            _queueSignal.Release();
        }


        /// <summary>
        /// Starts a long-running task that will dequeue items as they are queued and dispatch 
        /// them to either <see cref="RunBackgroundWorkItem(Action{CancellationToken}, CancellationToken)"/> 
        /// or <see cref="RunBackgroundWorkItem(Func{CancellationToken, Task}, CancellationToken)"/>.
        /// </summary>
        /// <param name="cancellationToken">
        ///   A cancellation token that will fire when the task should stop processing queued work 
        ///   items.
        /// </param>
        /// <returns>
        ///   A long-running task that will end when the <paramref name="cancellationToken"/> fires.
        /// </returns>
        /// <exception cref="OperationCanceledException">
        ///   The service is already running.
        /// </exception>
        public async Task RunAsync(CancellationToken cancellationToken) {
            ThrowIfDisposed();
            if (Interlocked.CompareExchange(ref _isRunning, 1, 0) != 0) {
                // Service is already running.
                throw new InvalidOperationException(Resources.Error_ServiceIsAlreadyRunning);
            }

            try {
                while (!cancellationToken.IsCancellationRequested) {
                    if (_isDisposed) {
                        break;
                    }

                    try {
                        await _queueSignal.WaitAsync(cancellationToken).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException) {
                        break;
                    }
                    catch (ArgumentNullException) {
                        // SemaphoreSlim on .NET Framework can throw an ArgumentNullException if it is 
                        // disposed during an asynchronous wait, because it attempts to lock on an object 
                        // that has been reset to null. See here for the code in question: 
                        // https://github.com/microsoft/referencesource/blob/17b97365645da62cf8a49444d979f94a59bbb155/mscorlib/system/threading/SemaphoreSlim.cs#L720
                        break;
                    }

                    if (!_queue.TryDequeue(out var item)) {
                        continue;
                    }

                    OnDequeued(item);

                    RunBackgroundWorkItem(item, cancellationToken);
                }
            }
            finally {
                _isRunning = 0;
            }
        }


        /// <summary>
        /// Runs a background work item.
        /// </summary>
        /// <param name="workItem">
        ///   The work item.
        /// </param>
        /// <param name="cancellationToken">
        ///   A cancellation token to pass to the <paramref name="workItem"/>.
        /// </param>
        protected abstract void RunBackgroundWorkItem(BackgroundWorkItem workItem, CancellationToken cancellationToken);


        /// <summary>
        /// Invokes the <see cref="BackgroundTaskServiceOptions.OnQueued"/> callback provided when 
        /// the service was registered.
        /// </summary>
        /// <param name="workItem">
        ///   The work item that was queued.
        /// </param>
        protected virtual void OnQueued(BackgroundWorkItem workItem) {
            try {
                _options.OnQueued?.Invoke(workItem);
            }
            catch (Exception e) {
                System.Diagnostics.Trace.TraceError(Resources.Trace_ErrorInCallback, nameof(BackgroundTaskServiceOptions.OnQueued), e.ToString());
            }
        }


        /// <summary>
        /// Invokes the <see cref="BackgroundTaskServiceOptions.OnDequeued"/> callback provided when 
        /// the service was registered.
        /// </summary>
        /// <param name="workItem">
        ///   The work item that was dequeued.
        /// </param>
        protected virtual void OnDequeued(BackgroundWorkItem workItem) {
            try {
                _options.OnQueued?.Invoke(workItem);
            }
            catch (Exception e) {
                System.Diagnostics.Trace.TraceError(Resources.Trace_ErrorInCallback, nameof(BackgroundTaskServiceOptions.OnDequeued), e.ToString());
            }
        }


        /// <summary>
        /// Invokes the <see cref="BackgroundTaskServiceOptions.OnRunning"/> callback provided when 
        /// the service was registered.
        /// </summary>
        /// <param name="workItem">
        ///   The work item that is being run.
        /// </param>
        protected virtual void OnRunning(BackgroundWorkItem workItem) {
            try {
                _options.OnRunning?.Invoke(workItem);
            }
            catch (Exception e) {
                System.Diagnostics.Trace.TraceError(Resources.Trace_ErrorInCallback, nameof(BackgroundTaskServiceOptions.OnRunning), e.ToString());
            }
        }


        /// <summary>
        /// Invokes the <see cref="BackgroundTaskServiceOptions.OnCompleted"/> callback provided when 
        /// the service was registered.
        /// </summary>
        /// <param name="workItem">
        ///   The work item that completed.
        /// </param>
        protected virtual void OnCompleted(BackgroundWorkItem workItem) {
            try {
                _options.OnCompleted?.Invoke(workItem);
            }
            catch (Exception e) {
                System.Diagnostics.Trace.TraceError(Resources.Trace_ErrorInCallback, nameof(BackgroundTaskServiceOptions.OnCompleted), e.ToString());
            }
        }


        /// <summary>
        /// Invokes the <see cref="BackgroundTaskServiceOptions.OnError"/> callback provided when 
        /// the service was registered.
        /// </summary>
        /// <param name="workItem">
        ///   The work item that raised the exception.
        /// </param>
        /// <param name="error">
        ///   The error that occurred.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="error"/> is <see langword="null"/>.
        /// </exception>
        protected virtual void OnError(BackgroundWorkItem workItem, Exception error) {
            if (error == null) {
                throw new ArgumentNullException(nameof(error));
            }

            try {
                _options.OnError?.Invoke(workItem, error);
            }
            catch (Exception e) {
                System.Diagnostics.Trace.TraceError(Resources.Trace_ErrorInCallback, nameof(BackgroundTaskServiceOptions.OnError), e.ToString());
            }
        }


        /// <summary>
        /// Releases managed and unmanaged resources.
        /// </summary>
        /// <param name="disposing">
        ///   <see langword="true"/> if the service is being disposed, or <see langword="false"/> 
        ///   if it is being finalized.
        /// </param>
        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                _queueSignal.Dispose();
#if NETSTANDARD2_0
                // .NET Standard 2.0 doesn't have a Clear() method on ConcurrentQueue<T>, so we'll 
                // empty it ourselves.
                while (_queue.TryDequeue(out var _)) { }
#else
                _queue.Clear();
#endif
            }
        }


        /// <inheritdoc/>
        public void Dispose() {
            if (_isDisposed) {
                return;
            }

            _isDisposed = true;
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        /// <summary>
        /// Class finalizer.
        /// </summary>
        ~BackgroundTaskService() {
            Dispose(false);
        }

    }

}
