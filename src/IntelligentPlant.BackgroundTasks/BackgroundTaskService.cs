using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace IntelligentPlant.BackgroundTasks {

    /// <summary>
    /// Base <see cref="IBackgroundTaskService"/> implementation. Call the <see cref="RunAsync"/> 
    /// method to start the service.
    /// </summary>
    public abstract partial class BackgroundTaskService : IBackgroundTaskService, IDisposable {

        /// <summary>
        /// The default background task service.
        /// </summary>
        private static readonly Lazy<IBackgroundTaskService> s_default = new Lazy<IBackgroundTaskService>(() => {
            var result = new DefaultBackgroundTaskService(null, null);
            _ = result.RunAsync(default);
            return result;
        }, LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// The default background task service, if the original default was externally overridden.
        /// </summary>
        private static IBackgroundTaskService? s_defaultOverridden;

        /// <summary>
        /// The default background task service.
        /// </summary>
        public static IBackgroundTaskService Default { 
            get { return s_defaultOverridden ?? s_default.Value; } 
            set {
                if (value != null && s_default.IsValueCreated && s_default.Value == value) {
                    s_defaultOverridden = null;
                }
                else {
                    s_defaultOverridden = value;
                }
            }
        }

        /// <summary>
        /// Logging.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Cancellation token source that fires when the service is being disposed.
        /// </summary>
        private readonly CancellationTokenSource _disposedCancellationTokenSource;

        /// <summary>
        /// Cancellation token that fires when the service is being disposed.
        /// </summary>
        private readonly CancellationToken _disposedCancellationToken;

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

        /// <inheritdoc/>
        public bool IsRunning { get { return _isRunning != 0; } }

        /// <inheritdoc/>
        public int QueuedItemCount { get { return _queue.Count; } }


        /// <summary>
        /// Creates a new <see cref="BackgroundTaskService"/> object.
        /// </summary>
        /// <param name="options">
        ///   The options for the service..
        /// </param>
        /// <param name="logger">
        ///   The <see cref="ILogger"/> for the service.
        /// </param>
        protected BackgroundTaskService(
            BackgroundTaskServiceOptions? options,
            ILogger? logger
        ) {
            _options = options ?? new BackgroundTaskServiceOptions();
            _disposedCancellationTokenSource = new CancellationTokenSource();
            _disposedCancellationToken = _disposedCancellationTokenSource.Token;
            _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
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

            if (!_options.AllowWorkItemRegistrationWhileStopped && !IsRunning) {
                throw new InvalidOperationException(Resources.Error_CannotRegisterWorkItemsWhileStopped);
            }

            if (workItem.WorkItem == null && workItem.WorkItemAsync == null) {
                // Default value; we'll ignore the item.
                return;
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
        ///   A long-running task that will end when the <paramref name="cancellationToken"/> 
        ///   fires or the <see cref="BackgroundTaskService"/> is disposed.
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
                LogServiceRunning(_logger);
                using (var compositeSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _disposedCancellationToken)) {
                    var compositeToken = compositeSource.Token;
                    while (!compositeToken.IsCancellationRequested) {
                        if (_isDisposed) {
                            break;
                        }

                        try {
                            await _queueSignal.WaitAsync(compositeToken).ConfigureAwait(false);
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

                        RunBackgroundWorkItem(item, compositeToken);
                    }
                }
            }
            finally {
                LogServiceStopped(_logger);
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
        /// Invokes the <see cref="BackgroundTaskServiceOptions.OnEnqueued"/> callback provided when 
        /// the service was registered.
        /// </summary>
        /// <param name="workItem">
        ///   The work item that was queued.
        /// </param>
        protected virtual void OnQueued(BackgroundWorkItem workItem) {
            try {
                LogItemEnqueued(_logger, workItem, IsRunning);
                _options.OnEnqueued?.Invoke(workItem);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e) {
#pragma warning restore CA1031 // Do not catch general exception types
                _logger.LogError(e, Resources.Log_ErrorInCallback, nameof(BackgroundTaskServiceOptions.OnEnqueued));
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
                LogItemDequeued(_logger, workItem);
                _options.OnDequeued?.Invoke(workItem);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e) {
#pragma warning restore CA1031 // Do not catch general exception types
                _logger.LogError(e, Resources.Log_ErrorInCallback, nameof(BackgroundTaskServiceOptions.OnDequeued));
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
                LogItemRunning(_logger, workItem);
                _options.OnRunning?.Invoke(workItem);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e) {
#pragma warning restore CA1031 // Do not catch general exception types
                _logger.LogError(e, Resources.Log_ErrorInCallback, nameof(BackgroundTaskServiceOptions.OnRunning));
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
                LogItemCompleted(_logger, workItem);
                _options.OnCompleted?.Invoke(workItem);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e) {
#pragma warning restore CA1031 // Do not catch general exception types
                _logger.LogError(e, Resources.Log_ErrorInCallback, nameof(BackgroundTaskServiceOptions.OnCompleted));
            }
        }


        /// <summary>
        /// Invokes the <see cref="BackgroundTaskServiceOptions.OnError"/> callback provided when 
        /// the service was registered.
        /// </summary>
        /// <param name="workItem">
        ///   The work item that raised the exception.
        /// </param>
        /// <param name="err">
        ///   The error that occurred.
        /// </param>
        protected virtual void OnError(BackgroundWorkItem workItem, Exception err) {
            if (err == null) {
                err = new Exception(Resources.Error_UnspecifiedError);
            }

            try {
                LogItemFaulted(_logger, workItem, err);
                _options.OnError?.Invoke(workItem, err);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e) {
#pragma warning restore CA1031 // Do not catch general exception types
                _logger.LogError(e, Resources.Log_ErrorInCallback, nameof(BackgroundTaskServiceOptions.OnError));
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
            if (_isDisposed) {
                return;
            }

            if (disposing) {
                _disposedCancellationTokenSource.Cancel();
                _disposedCancellationTokenSource.Dispose();
                _queueSignal.Dispose();
#if !NETSTANDARD2_1
                // .NET Framework 4.6.1 / .NET Standard 2.0 don't have a Clear() method on ConcurrentQueue<T>, so we'll 
                // empty it ourselves.
                while (_queue.TryDequeue(out var _)) { }
#else
                _queue.Clear();
#endif
            }

            _isDisposed = true;
        }


        /// <inheritdoc/>
        public void Dispose() {
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
