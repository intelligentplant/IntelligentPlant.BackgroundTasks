﻿using System;
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
        /// Callback to invoke when an error occurs while running a background task.
        /// </summary>
        private readonly Action<Exception, BackgroundWorkItem> _onError;

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
            _onError = options?.OnError;
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
        public void QueueBackgroundWorkItem(Action<CancellationToken> workItem) {
            ThrowIfDisposed();

            if (workItem == null) {
                throw new ArgumentNullException(nameof(workItem));
            }
            _queue.Enqueue(new BackgroundWorkItem(workItem));
            _queueSignal.Release();
        }


        /// <inheritdoc/>
        public void QueueBackgroundWorkItem(Func<CancellationToken, Task> workItem) {
            ThrowIfDisposed();

            if (workItem == null) {
                throw new ArgumentNullException(nameof(workItem));
            }
            _queue.Enqueue(new BackgroundWorkItem(workItem));
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

                    if (item.WorkItem != null) {
                        RunBackgroundWorkItem(item.WorkItem, cancellationToken);
                    }
                    else if (item.WorkItemAsync != null) {
                        RunBackgroundWorkItem(item.WorkItemAsync, cancellationToken);
                    }
                }
            }
            finally {
                _isRunning = 0;
            }
        }


        /// <summary>
        /// Runs a synchronous background work item.
        /// </summary>
        /// <param name="workItem">
        ///   The work item.
        /// </param>
        /// <param name="cancellationToken">
        ///   A cancellation token to pass to the <paramref name="workItem"/>.
        /// </param>
        protected abstract void RunBackgroundWorkItem(Action<CancellationToken> workItem, CancellationToken cancellationToken);


        /// <summary>
        /// Runs an asynchronous background work item.
        /// </summary>
        /// <param name="workItem">
        ///   The work item.
        /// </param>
        /// <param name="cancellationToken">
        ///   A cancellation token to pass to the <paramref name="workItem"/>.
        /// </param>
        protected abstract void RunBackgroundWorkItem(Func<CancellationToken, Task> workItem, CancellationToken cancellationToken);


        /// <summary>
        /// Invokes the <see cref="BackgroundTaskServiceOptions.OnError"/> callback provided when 
        /// the service was registered.
        /// </summary>
        /// <param name="error">
        ///   The error that occurred.
        /// </param>
        /// <param name="workItem">
        ///   The work item that raised the exception.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="error"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="workItem"/> is <see langword="null"/>.
        /// </exception>
        protected virtual void OnError(Exception error, Action<CancellationToken> workItem) {
            if (error == null) {
                throw new ArgumentNullException(nameof(error));
            }
            if (workItem == null) {
                throw new ArgumentNullException(nameof(workItem));
            }
            _onError?.Invoke(error, new BackgroundWorkItem(workItem));
        }


        /// <summary>
        /// Invokes the <see cref="BackgroundTaskServiceOptions.OnError"/> callback provided when 
        /// the service was registered.
        /// </summary>
        /// <param name="error">
        ///   The error that occurred.
        /// </param>
        /// <param name="workItem">
        ///   The work item that raised the exception.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="error"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="workItem"/> is <see langword="null"/>.
        /// </exception>
        protected virtual void OnError(Exception error, Func<CancellationToken, Task> workItem) {
            if (workItem == null) {
                throw new ArgumentNullException(nameof(workItem));
            }
            _onError?.Invoke(error, new BackgroundWorkItem(workItem));
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
