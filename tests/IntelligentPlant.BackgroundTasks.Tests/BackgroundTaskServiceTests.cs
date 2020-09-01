using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IntelligentPlant.BackgroundTasks.Tests {
    [TestClass]
    public class BackgroundTaskServiceTests {

        private static IServiceProvider s_serviceProvider;

        public TestContext TestContext { get; set; }


        [ClassInitialize]
        public static void ClassInitialize(TestContext context) {
            var services = new ServiceCollection();
            services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Trace));
            s_serviceProvider = services.BuildServiceProvider();
        }


        [TestMethod]
        public void TaskShouldBeEnqueued() {

            using (var svc = new DefaultBackgroundTaskService(null, s_serviceProvider.GetRequiredService<ILogger<DefaultBackgroundTaskService>>())) {
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
            using (var svc = new DefaultBackgroundTaskService(null, s_serviceProvider.GetRequiredService<ILogger<DefaultBackgroundTaskService>>())) {
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

                var svc = new DefaultBackgroundTaskService(options, s_serviceProvider.GetRequiredService<ILogger<DefaultBackgroundTaskService>>());
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
            using (var svc = new DefaultBackgroundTaskService(null, s_serviceProvider.GetRequiredService<ILogger<DefaultBackgroundTaskService>>())) {
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


        [TestMethod]
        public void DescriptionShouldBeAutoGeneratedForSyncWorkItem() {
            string? descriptionActual;

            var options = new BackgroundTaskServiceOptions() {
                OnQueued = item => descriptionActual = item.Description
            };

            using (var ctSource1 = new CancellationTokenSource())
            using (var svc = new DefaultBackgroundTaskService(options, s_serviceProvider.GetRequiredService<ILogger<DefaultBackgroundTaskService>>())) {
                Action<CancellationToken> func = ct => { };

                // Test when queuing with no additional cancellation tokens.
                descriptionActual = null;
                svc.QueueBackgroundWorkItem(func);
                Assert.AreEqual(1, svc.QueuedItemCount);
                Assert.AreEqual(BackgroundWorkItem.CreateDescriptionFromDelegate(func), descriptionActual);

                // Test when queuing with additional cancellation tokens.
                descriptionActual = null;
                svc.QueueBackgroundWorkItem(func, ctSource1.Token);
                Assert.AreEqual(2, svc.QueuedItemCount);
                Assert.AreEqual(BackgroundWorkItem.CreateDescriptionFromDelegate(func), descriptionActual);
            }
        }


        [TestMethod]
        public void DescriptionShouldBeAutoGeneratedForAsyncWorkItem() {
            string? descriptionActual;

            var options = new BackgroundTaskServiceOptions() {
                OnQueued = item => descriptionActual = item.Description
            };

            using (var ctSource1 = new CancellationTokenSource())
            using (var svc = new DefaultBackgroundTaskService(options, s_serviceProvider.GetRequiredService<ILogger<DefaultBackgroundTaskService>>())) {
                Func<CancellationToken, Task> func = ct => Task.CompletedTask;

                // Test when queuing with no additional cancellation tokens.
                descriptionActual = null;
                svc.QueueBackgroundWorkItem(func);
                Assert.AreEqual(1, svc.QueuedItemCount);
                Assert.AreEqual(BackgroundWorkItem.CreateDescriptionFromDelegate(func), descriptionActual);

                // Test when queuing with additional cancellation tokens.
                descriptionActual = null;
                svc.QueueBackgroundWorkItem(func, ctSource1.Token);
                Assert.AreEqual(2, svc.QueuedItemCount);
                Assert.AreEqual(BackgroundWorkItem.CreateDescriptionFromDelegate(func), descriptionActual);
            }
        }


        [TestMethod]
        public void DescriptionShouldNotBeAutoGeneratedForSyncWorkItem() {
            string? descriptionActual;

            var options = new BackgroundTaskServiceOptions() {
                OnQueued = item => descriptionActual = item.Description
            };

            using (var ctSource1 = new CancellationTokenSource())
            using (var svc = new DefaultBackgroundTaskService(options, s_serviceProvider.GetRequiredService<ILogger<DefaultBackgroundTaskService>>())) {
                Action<CancellationToken> func = ct => { };

                // Test when queuing with no additional cancellation tokens.
                descriptionActual = null;
                svc.QueueBackgroundWorkItem(func, TestContext.TestName);
                Assert.AreEqual(1, svc.QueuedItemCount);
                Assert.AreEqual(TestContext.TestName, descriptionActual);

                // Test when queuing with additional cancellation tokens.
                descriptionActual = null;
                svc.QueueBackgroundWorkItem(func, TestContext.TestName, ctSource1.Token);
                Assert.AreEqual(2, svc.QueuedItemCount);
                Assert.AreEqual(TestContext.TestName, descriptionActual);
            }
        }


        [TestMethod]
        public void DescriptionShouldNotBeAutoGeneratedForAsyncWorkItem() {
            string? descriptionActual;

            var options = new BackgroundTaskServiceOptions() {
                OnQueued = item => descriptionActual = item.Description
            };

            using (var ctSource1 = new CancellationTokenSource())
            using (var svc = new DefaultBackgroundTaskService(options, s_serviceProvider.GetRequiredService<ILogger<DefaultBackgroundTaskService>>())) {
                Func<CancellationToken, Task> func = ct => Task.CompletedTask;

                // Test when queuing with no additional cancellation tokens.
                descriptionActual = null;
                svc.QueueBackgroundWorkItem(func, TestContext.TestName);
                Assert.AreEqual(1, svc.QueuedItemCount);
                Assert.AreEqual(TestContext.TestName, descriptionActual);

                // Test when queuing with additional cancellation tokens.
                descriptionActual = null;
                svc.QueueBackgroundWorkItem(func, TestContext.TestName, ctSource1.Token);
                Assert.AreEqual(2, svc.QueuedItemCount);
                Assert.AreEqual(TestContext.TestName, descriptionActual);
            }
        }

    }
}
