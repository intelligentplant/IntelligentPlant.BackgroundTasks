﻿using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IntelligentPlant.BackgroundTasks.Tests {
    [TestClass]
    public class BackgroundTaskServiceTests {

        private static IServiceProvider s_serviceProvider = default!;

        public TestContext TestContext { get; set; } = default!;


        [ClassInitialize]
        public static void ClassInitialize(TestContext context) {
            var services = new ServiceCollection();
            services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Trace));
            s_serviceProvider = services.BuildServiceProvider();
        }


        [TestMethod]
        public void TaskShouldBeEnqueuedWhenServiceIsStopped() {
            var options = new BackgroundTaskServiceOptions() {
                AllowWorkItemRegistrationWhileStopped = true,
            };

            using (var svc = ActivatorUtilities.CreateInstance<DefaultBackgroundTaskService>(s_serviceProvider, options)) {
                svc.QueueBackgroundWorkItem(ct => { 
                    // No-op
                });

                Assert.AreEqual(1, svc.QueuedItemCount);
            }

        }


        [TestMethod]
        public void TaskShouldNotBeEnqueuedWhenServiceIsStopped() {
            var options = new BackgroundTaskServiceOptions() {
                AllowWorkItemRegistrationWhileStopped = false,
            };

            using (var svc = ActivatorUtilities.CreateInstance<DefaultBackgroundTaskService>(s_serviceProvider, options)) {
                Assert.ThrowsException<InvalidOperationException>(() => svc.QueueBackgroundWorkItem(ct => {
                    // No-op
                }));

                Assert.AreEqual(0, svc.QueuedItemCount);
            }

        }


        [TestMethod]
        public async Task TaskShouldRunOnDefaultService() {
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

            var options = new BackgroundTaskServiceOptions() {
                AllowWorkItemRegistrationWhileStopped = true,
            };

            using (var svc = ActivatorUtilities.CreateInstance<DefaultBackgroundTaskService>(s_serviceProvider, options))
            using (var semaphore = new SemaphoreSlim(0))
            using (var ctSource = new CancellationTokenSource()) {
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
                    AllowWorkItemRegistrationWhileStopped = true,
                    OnError = (error, workItem) => {
                        value = 1;
                        semaphore.Release();
                    }
                };

                var svc = ActivatorUtilities.CreateInstance<DefaultBackgroundTaskService>(s_serviceProvider, options);
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

            var options = new BackgroundTaskServiceOptions() {
                AllowWorkItemRegistrationWhileStopped = true,
            };

            using (var svc = ActivatorUtilities.CreateInstance<DefaultBackgroundTaskService>(s_serviceProvider, options))
            using (var semaphore = new SemaphoreSlim(0))
            using (var ctSource1 = new CancellationTokenSource())
            using (var ctSource2 = new CancellationTokenSource()) {
                svc.QueueBackgroundWorkItem(async ct => {
                    try {
                        await Task.Delay(Timeout.Infinite, ct);
                    }
                    catch { }
                    finally {
                        value = 1;
                        semaphore.Release();
                    }
                }, null, false, ctSource2.Token);

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
        public async Task ParentActivityShouldBeCapturedInSyncDelegate() {
            var value = 0;

            var options = new BackgroundTaskServiceOptions() {
                AllowWorkItemRegistrationWhileStopped = true,
            };

            using (var activitySourceListener = new ActivityListener() { 
                ShouldListenTo = x => true,
                SampleUsingParentId = (ref ActivityCreationOptions<string> activityOptions) => ActivitySamplingResult.AllData,
                Sample = (ref ActivityCreationOptions<ActivityContext> activityOptions) => ActivitySamplingResult.AllData
            })
            using (var activitySource = new ActivitySource(TestContext.TestName))
            using (var svc = ActivatorUtilities.CreateInstance<DefaultBackgroundTaskService>(s_serviceProvider, options))
            using (var semaphore = new SemaphoreSlim(0))
            using (var ctSource = new CancellationTokenSource()) {
                var run = svc.RunAsync(ctSource.Token);
                ActivitySource.AddActivityListener(activitySourceListener);

                using (var parentActivity = activitySource.StartActivity("Parent")) {
                    svc.QueueBackgroundWorkItem(ct => {
                        using (activitySource.StartActivity(TestContext.TestName)) {
                            try {
                                Assert.AreEqual(TestContext.TestName, Activity.Current?.DisplayName);
                                Assert.AreEqual(parentActivity!.DisplayName, Activity.Current?.Parent?.DisplayName);

                                value = 1;
                            }
                            finally {
                                semaphore.Release();
                            }
                        }
                    }, null, true);

                    Assert.AreEqual(1, svc.QueuedItemCount);

                    var lockObtained = await semaphore.WaitAsync(5000);

                    Assert.IsTrue(lockObtained);
                    Assert.AreEqual(0, svc.QueuedItemCount);
                    Assert.AreEqual(1, value);
                    Assert.AreEqual(parentActivity?.DisplayName, Activity.Current?.DisplayName);

                    ctSource.Cancel();
                    await run;
                }
            }
        }


        [TestMethod]
        public async Task ParentActivityShouldNotBeCapturedInSyncDelegate() {
            var value = 0;

            var options = new BackgroundTaskServiceOptions() {
                AllowWorkItemRegistrationWhileStopped = true,
            };

            using (var activitySourceListener = new ActivityListener() {
                ShouldListenTo = x => true,
                SampleUsingParentId = (ref ActivityCreationOptions<string> activityOptions) => ActivitySamplingResult.AllData,
                Sample = (ref ActivityCreationOptions<ActivityContext> activityOptions) => ActivitySamplingResult.AllData
            })
            using (var activitySource = new ActivitySource(TestContext.TestName))
            using (var svc = ActivatorUtilities.CreateInstance<DefaultBackgroundTaskService>(s_serviceProvider, options))
            using (var semaphore = new SemaphoreSlim(0))
            using (var ctSource = new CancellationTokenSource()) {
                var run = svc.RunAsync(ctSource.Token);
                ActivitySource.AddActivityListener(activitySourceListener);

                using (var parentActivity = activitySource.StartActivity("Parent")) {
                    svc.QueueBackgroundWorkItem(ct => {
                        using (activitySource.StartActivity(TestContext.TestName)) {
                            try {
                                Assert.AreEqual(TestContext.TestName, Activity.Current?.DisplayName);
                                Assert.AreNotEqual(parentActivity!.DisplayName, Activity.Current?.Parent?.DisplayName);

                                value = 1;
                            }
                            finally {
                                semaphore.Release();
                            }
                        }
                    }, null, false);

                    Assert.AreEqual(1, svc.QueuedItemCount);

                    var lockObtained = await semaphore.WaitAsync(5000);

                    Assert.IsTrue(lockObtained);
                    Assert.AreEqual(0, svc.QueuedItemCount);
                    Assert.AreEqual(1, value);
                    Assert.AreEqual(parentActivity?.DisplayName, Activity.Current?.DisplayName);

                    ctSource.Cancel();
                    await run;
                }
            }
        }


        [TestMethod]
        public async Task ParentActivityShouldBeCapturedInAsyncDelegate() {
            var value = 0;

            var options = new BackgroundTaskServiceOptions() {
                AllowWorkItemRegistrationWhileStopped = true,
            };

            using (var activitySourceListener = new ActivityListener() {
                ShouldListenTo = x => true,
                SampleUsingParentId = (ref ActivityCreationOptions<string> activityOptions) => ActivitySamplingResult.AllData,
                Sample = (ref ActivityCreationOptions<ActivityContext> activityOptions) => ActivitySamplingResult.AllData
            })
            using (var activitySource = new ActivitySource(TestContext.TestName))
            using (var svc = ActivatorUtilities.CreateInstance<DefaultBackgroundTaskService>(s_serviceProvider, options))
            using (var semaphore = new SemaphoreSlim(0))
            using (var ctSource = new CancellationTokenSource()) {
                var run = svc.RunAsync(ctSource.Token);
                ActivitySource.AddActivityListener(activitySourceListener);

                using (var parentActivity = activitySource.StartActivity("Parent")) {
                    svc.QueueBackgroundWorkItem(async ct => {
                        using (activitySource.StartActivity(TestContext.TestName)) {
                            try {
                                await Task.Yield();
                                Assert.AreEqual(TestContext.TestName, Activity.Current?.DisplayName);
                                Assert.AreEqual(parentActivity!.DisplayName, Activity.Current?.Parent?.DisplayName);

                                value = 1;
                            }
                            finally {
                                semaphore.Release();
                            }
                        }
                    }, null, true);

                    Assert.AreEqual(1, svc.QueuedItemCount);

                    var lockObtained = await semaphore.WaitAsync(5000);

                    Assert.IsTrue(lockObtained);
                    Assert.AreEqual(0, svc.QueuedItemCount);
                    Assert.AreEqual(1, value);
                    Assert.AreEqual(parentActivity?.DisplayName, Activity.Current?.DisplayName);

                    ctSource.Cancel();
                    await run;
                }
            }
        }


        [TestMethod]
        public async Task ParentActivityShouldNotBeCapturedInAsyncDelegate() {
            var value = 0;

            var options = new BackgroundTaskServiceOptions() {
                AllowWorkItemRegistrationWhileStopped = true,
            };

            using (var activitySourceListener = new ActivityListener() {
                ShouldListenTo = x => true,
                SampleUsingParentId = (ref ActivityCreationOptions<string> activityOptions) => ActivitySamplingResult.AllData,
                Sample = (ref ActivityCreationOptions<ActivityContext> activityOptions) => ActivitySamplingResult.AllData
            })
            using (var activitySource = new ActivitySource(TestContext.TestName))
            using (var svc = ActivatorUtilities.CreateInstance<DefaultBackgroundTaskService>(s_serviceProvider, options))
            using (var semaphore = new SemaphoreSlim(0))
            using (var ctSource = new CancellationTokenSource()) {
                var run = svc.RunAsync(ctSource.Token);
                ActivitySource.AddActivityListener(activitySourceListener);

                using (var parentActivity = activitySource.StartActivity("Parent")) {
                    svc.QueueBackgroundWorkItem(async ct => {
                        using (activitySource.StartActivity(TestContext.TestName)) {
                            try {
                                await Task.Yield();
                                Assert.AreEqual(TestContext.TestName, Activity.Current?.DisplayName);
                                Assert.AreNotEqual(parentActivity!.DisplayName, Activity.Current?.Parent?.DisplayName);

                                value = 1;
                            }
                            finally {
                                semaphore.Release();
                            }
                        }
                    }, null, false);

                    Assert.AreEqual(1, svc.QueuedItemCount);

                    var lockObtained = await semaphore.WaitAsync(5000);

                    Assert.IsTrue(lockObtained);
                    Assert.AreEqual(0, svc.QueuedItemCount);
                    Assert.AreEqual(1, value);
                    Assert.AreEqual(parentActivity?.DisplayName, Activity.Current?.DisplayName);

                    ctSource.Cancel();
                    await run;
                }
            }
        }

    }
}
