using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IntelligentPlant.BackgroundTasks.Tests {
    [TestClass]
    public class BackgroundTaskServiceTests {

        [TestMethod]
        public void TaskShouldBeEnqueued() {

            using (var svc = new DefaultBackgroundTaskService(null)) {
                svc.QueueBackgroundWorkItem(ct => { 
                    // No-op
                });

                Assert.AreEqual(1, svc.QueuedItemCount);
            }

        }


        [TestMethod]
        public async Task TaskShouldRunOnDefaultScheduler() {
            var value = 0;

            using (var semaphore = new SemaphoreSlim(0)) {
                BackgroundTaskService.Default.QueueBackgroundWorkItem(ct => {
                    value = 1;
                    semaphore.Release();
                });

                var lockObtained = await semaphore.WaitAsync(5000);

                Assert.IsTrue(lockObtained);
                Assert.AreEqual(1, value);
            }

        }


        [TestMethod]
        public async Task TaskShouldRun() {
            var value = 0;

            using (var semaphore = new SemaphoreSlim(0))
            using (var ctSource = new CancellationTokenSource())
            using (var svc = new DefaultBackgroundTaskService(null)) {
                svc.QueueBackgroundWorkItem(ct => {
                    value = 1;
                    semaphore.Release();
                });

                Assert.AreEqual(1, svc.QueuedItemCount);

                var run = svc.RunAsync(ctSource.Token);

                var lockObtained = await semaphore.WaitAsync(5000);

                Assert.IsTrue(lockObtained);
                Assert.AreEqual(0, svc.QueuedItemCount);
                Assert.AreEqual(1, value);

                ctSource.Cancel();
                await run;
            }

        }


        [TestMethod]
        public async Task TaskExceptionShouldBeCaught() {
            var value = 0;

            using (var semaphore = new SemaphoreSlim(0))
            using (var ctSource = new CancellationTokenSource()) {
                var options = new BackgroundTaskServiceOptions() {
                    OnError = (error, workItem) => {
                        value = 1;
                        semaphore.Release();
                    }
                };

                var svc = new DefaultBackgroundTaskService(options);
                try {
                    svc.QueueBackgroundWorkItem(ct => throw new InvalidOperationException());

                    Assert.AreEqual(1, svc.QueuedItemCount);

                    var run = svc.RunAsync(ctSource.Token);

                    var lockObtained = await semaphore.WaitAsync(5000);

                    Assert.IsTrue(lockObtained);
                    Assert.AreEqual(0, svc.QueuedItemCount);
                    Assert.AreEqual(1, value);

                    ctSource.Cancel();
                    await run;
                }
                finally {
                    svc.Dispose();
                }
            }
        }


        [TestMethod]
        public async Task TaskShouldObeyAdditionalCancellatonTokens() {
            var value = 0;

            using (var semaphore = new SemaphoreSlim(0))
            using (var ctSource1 = new CancellationTokenSource())
            using (var ctSource2 = new CancellationTokenSource())
            using (var svc = new DefaultBackgroundTaskService(null)) {
                svc.QueueBackgroundWorkItem(async ct => {
                    try {
                        await Task.Delay(Timeout.Infinite, ct);
                    }
                    catch { }
                    finally {
                        value = 1;
                        semaphore.Release();
                    }
                }, null, ctSource2.Token);

                Assert.AreEqual(1, svc.QueuedItemCount);

                var run = svc.RunAsync(ctSource1.Token);

                ctSource2.Cancel();
                var lockObtained = await semaphore.WaitAsync(5000);

                Assert.IsTrue(lockObtained);
                Assert.AreEqual(0, svc.QueuedItemCount);
                Assert.AreEqual(1, value);

                ctSource1.Cancel();
                await run;
            }
        }

    }
}
