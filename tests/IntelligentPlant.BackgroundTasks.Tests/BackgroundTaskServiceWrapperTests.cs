using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IntelligentPlant.BackgroundTasks.Tests {

    [TestClass]
    public class BackgroundTaskServiceWrapperTests {

        [TestMethod]
        public async Task WorkItemsShouldObeyWrapperCancellationToken() {
            var ctSource = new CancellationTokenSource();
            var tcs = new TaskCompletionSource<int>();

            using (IBackgroundTaskService wrapper = new BackgroundTaskServiceWrapper(BackgroundTaskService.Default, () => ctSource.Token))
            using (var testTimeout = new CancellationTokenSource(5000)) {
                wrapper.QueueBackgroundWorkItem(async ct => {
                    try {
                        await Task.Delay(-1, ct).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException) {
                        if (testTimeout.IsCancellationRequested) {
                            tcs.TrySetCanceled(testTimeout.Token);
                        }
                    }
                    finally {
                        tcs.TrySetResult(0);
                    }
                }, testTimeout.Token);

                ctSource.CancelAfter(100);
                await tcs.Task.ConfigureAwait(false);
            }
        }

    }

}
