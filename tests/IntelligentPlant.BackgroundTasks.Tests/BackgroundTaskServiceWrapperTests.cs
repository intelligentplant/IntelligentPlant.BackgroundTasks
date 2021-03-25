using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IntelligentPlant.BackgroundTasks.Tests {

    [TestClass]
    public class BackgroundTaskServiceWrapperTests {

        public TestContext TestContext { get; set; } = default!;


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
                }, null, null, false, testTimeout.Token);

                ctSource.CancelAfter(100);
                await tcs.Task.ConfigureAwait(false);
            }
        }


        [TestMethod]
        public async Task ParentActivityShouldBeCapturedInSyncDelegate() {
            var tcs = new TaskCompletionSource<int>();

            var options = new BackgroundTaskServiceOptions() {
                AllowWorkItemRegistrationWhileStopped = true,
            };

            using (var ctSource = new CancellationTokenSource())
            using (var activitySourceListener = new ActivityListener() {
                ShouldListenTo = x => true,
                SampleUsingParentId = (ref ActivityCreationOptions<string> activityOptions) => ActivitySamplingResult.AllData,
                Sample = (ref ActivityCreationOptions<ActivityContext> activityOptions) => ActivitySamplingResult.AllData
            })
            using (var activitySource = new ActivitySource(TestContext.TestName))
            using (IBackgroundTaskService svc = new BackgroundTaskServiceWrapper(BackgroundTaskService.Default, () => ctSource.Token)) {
                ActivitySource.AddActivityListener(activitySourceListener);

                using (var parentActivity = activitySource.StartActivity("Parent")) {
                    svc.QueueBackgroundWorkItem(ct => {
                        try {
                            Assert.AreEqual(TestContext.TestName, Activity.Current?.DisplayName);
                            Assert.AreEqual(parentActivity!.DisplayName, Activity.Current?.Parent?.DisplayName);

                            tcs.TrySetResult(1);
                        }
                        catch (Exception e) {
                            tcs.TrySetException(e);
                        }
                    }, null, () => activitySource.StartActivity(TestContext.TestName), true);

                    ctSource.CancelAfter(5000);
                    var value = await tcs.Task;

                    Assert.AreEqual(1, value);
                    Assert.AreEqual(parentActivity?.DisplayName, Activity.Current?.DisplayName);

                }
            }
        }


        [TestMethod]
        public async Task ParentActivityShouldNotBeCapturedInSyncDelegate() {
            var tcs = new TaskCompletionSource<int>();

            var options = new BackgroundTaskServiceOptions() {
                AllowWorkItemRegistrationWhileStopped = true,
            };

            using (var ctSource = new CancellationTokenSource())
            using (var activitySourceListener = new ActivityListener() {
                ShouldListenTo = x => true,
                SampleUsingParentId = (ref ActivityCreationOptions<string> activityOptions) => ActivitySamplingResult.AllData,
                Sample = (ref ActivityCreationOptions<ActivityContext> activityOptions) => ActivitySamplingResult.AllData
            })
            using (var activitySource = new ActivitySource(TestContext.TestName))
            using (IBackgroundTaskService svc = new BackgroundTaskServiceWrapper(BackgroundTaskService.Default, () => ctSource.Token)) {
                ActivitySource.AddActivityListener(activitySourceListener);

                using (var parentActivity = activitySource.StartActivity("Parent")) {
                    svc.QueueBackgroundWorkItem(ct => {
                        try {
                            Assert.AreEqual(TestContext.TestName, Activity.Current?.DisplayName);
                            Assert.AreNotEqual(parentActivity!.DisplayName, Activity.Current?.Parent?.DisplayName);

                            tcs.TrySetResult(1);
                        }
                        catch (Exception e) {
                            tcs.TrySetException(e);
                        }
                    }, null, () => activitySource.StartActivity(TestContext.TestName), false);

                    ctSource.CancelAfter(5000);
                    var value = await tcs.Task;

                    Assert.AreEqual(1, value);
                    Assert.AreEqual(parentActivity?.DisplayName, Activity.Current?.DisplayName);

                }
            }
        }


        [TestMethod]
        public async Task ParentActivityShouldBeCapturedInAsyncDelegate() {
            var tcs = new TaskCompletionSource<int>();

            var options = new BackgroundTaskServiceOptions() {
                AllowWorkItemRegistrationWhileStopped = true,
            };

            using (var ctSource = new CancellationTokenSource())
            using (var activitySourceListener = new ActivityListener() {
                ShouldListenTo = x => true,
                SampleUsingParentId = (ref ActivityCreationOptions<string> activityOptions) => ActivitySamplingResult.AllData,
                Sample = (ref ActivityCreationOptions<ActivityContext> activityOptions) => ActivitySamplingResult.AllData
            })
            using (var activitySource = new ActivitySource(TestContext.TestName))
            using (IBackgroundTaskService svc = new BackgroundTaskServiceWrapper(BackgroundTaskService.Default, () => ctSource.Token)) {
                ActivitySource.AddActivityListener(activitySourceListener);

                using (var parentActivity = activitySource.StartActivity("Parent")) {
                    svc.QueueBackgroundWorkItem(async ct => {
                        try {
                            await Task.Yield();
                            Assert.AreEqual(TestContext.TestName, Activity.Current?.DisplayName);
                            Assert.AreEqual(parentActivity!.DisplayName, Activity.Current?.Parent?.DisplayName);

                            tcs.TrySetResult(1);
                        }
                        catch (Exception e) {
                            tcs.TrySetException(e);
                        }
                    }, null, () => activitySource.StartActivity(TestContext.TestName), true);

                    ctSource.CancelAfter(5000);
                    var value = await tcs.Task;

                    Assert.AreEqual(1, value);
                    Assert.AreEqual(parentActivity?.DisplayName, Activity.Current?.DisplayName);

                }
            }
        }


        [TestMethod]
        public async Task ParentActivityShouldNotBeCapturedInAsyncDelegate() {
            var tcs = new TaskCompletionSource<int>();

            var options = new BackgroundTaskServiceOptions() {
                AllowWorkItemRegistrationWhileStopped = true,
            };

            using (var ctSource = new CancellationTokenSource())
            using (var activitySourceListener = new ActivityListener() {
                ShouldListenTo = x => true,
                SampleUsingParentId = (ref ActivityCreationOptions<string> activityOptions) => ActivitySamplingResult.AllData,
                Sample = (ref ActivityCreationOptions<ActivityContext> activityOptions) => ActivitySamplingResult.AllData
            })
            using (var activitySource = new ActivitySource(TestContext.TestName))
            using (IBackgroundTaskService svc = new BackgroundTaskServiceWrapper(BackgroundTaskService.Default, () => ctSource.Token)) {
                ActivitySource.AddActivityListener(activitySourceListener);

                using (var parentActivity = activitySource.StartActivity("Parent")) {
                    svc.QueueBackgroundWorkItem(async ct => {
                        try {
                            await Task.Yield();
                            Assert.AreEqual(TestContext.TestName, Activity.Current?.DisplayName);
                            Assert.AreNotEqual(parentActivity!.DisplayName, Activity.Current?.Parent?.DisplayName);

                            tcs.TrySetResult(1);
                        }
                        catch (Exception e) {
                            tcs.TrySetException(e);
                        }
                    }, null, () => activitySource.StartActivity(TestContext.TestName), false);

                    ctSource.CancelAfter(5000);
                    var value = await tcs.Task;

                    Assert.AreEqual(1, value);
                    Assert.AreEqual(parentActivity?.DisplayName, Activity.Current?.DisplayName);

                }
            }
        }

    }

}
