using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IntelligentPlant.BackgroundTasks.Tests {

    [TestClass]
    public class EventSourceTests {

        private static IServiceProvider s_serviceProvider = default!;

        public TestContext TestContext { get; set; } = default!;


        [ClassInitialize]
        public static void ClassInitialize(TestContext context) {
            var services = new ServiceCollection();
            services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Trace));
            s_serviceProvider = services.BuildServiceProvider();
        }


        [TestMethod]
        public async Task ShouldEmitServiceRunningEvent() {
            var options = new BackgroundTaskServiceOptions() {
                AllowWorkItemRegistrationWhileStopped = true,
                Name = TestContext.TestName
            };

            using (var svc = ActivatorUtilities.CreateInstance<DefaultBackgroundTaskService>(s_serviceProvider, options))
            using (var ctSource = new CancellationTokenSource(5000)) {
                var tcs = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
                ctSource.Token.Register(() => tcs.TrySetCanceled());

                using (var listener = new Listener(eventData => { 
                    if (eventData.EventId == BackgroundTaskService.EventCodes.ServiceRunning) {
                        tcs.TrySetResult(0);
                    }
                })) {
                    _ = svc.RunAsync(ctSource.Token);
                    await tcs.Task;
                }
            }
        }


        [TestMethod]
        public async Task ShouldEmitServiceStoppedEvent() {
            var options = new BackgroundTaskServiceOptions() {
                AllowWorkItemRegistrationWhileStopped = true,
                Name = TestContext.TestName
            };

            using (var svc = ActivatorUtilities.CreateInstance<DefaultBackgroundTaskService>(s_serviceProvider, options))
            using (var ctSource = new CancellationTokenSource(5000))
            using (var ctSource2 = new CancellationTokenSource()) {
                var tcs = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
                ctSource.Token.Register(() => tcs.TrySetCanceled());

                using (var listener = new Listener(eventData => {
                    if (eventData.EventId == BackgroundTaskService.EventCodes.ServiceStopped) {
                        tcs.TrySetResult(0);
                    }
                })) {
                    _ = svc.RunAsync(ctSource2.Token);
                    ctSource2.Cancel();
                    await tcs.Task;
                }
            }
        }


        [DataTestMethod]
        [DataRow(BackgroundTaskService.EventCodes.WorkItemEnqueued)]
        [DataRow(BackgroundTaskService.EventCodes.WorkItemDequeued)]
        [DataRow(BackgroundTaskService.EventCodes.WorkItemRunning)]
        [DataRow(BackgroundTaskService.EventCodes.WorkItemCompleted)]
        public async Task ShouldEmitCompletedWorkItemLifecycleEvents(int eventCode) {
            var options = new BackgroundTaskServiceOptions() {
                AllowWorkItemRegistrationWhileStopped = true,
                Name = TestContext.TestName + "_" + eventCode
            };

            using (var svc = ActivatorUtilities.CreateInstance<DefaultBackgroundTaskService>(s_serviceProvider, options))
            using (var ctSource = new CancellationTokenSource(5000)) {
                var tcs = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
                ctSource.Token.Register(() => tcs.TrySetCanceled());

                using (var listener = new Listener(eventData => {
                    if (eventData.EventId == eventCode) {
                        tcs.TrySetResult(0);
                    }
                })) {
                    _ = svc.RunAsync(ctSource.Token);
                    svc.QueueBackgroundWorkItem(ct => { });
                    await tcs.Task;
                }
            }
        }


        [DataTestMethod]
        [DataRow(BackgroundTaskService.EventCodes.WorkItemEnqueued)]
        [DataRow(BackgroundTaskService.EventCodes.WorkItemDequeued)]
        [DataRow(BackgroundTaskService.EventCodes.WorkItemRunning)]
        [DataRow(BackgroundTaskService.EventCodes.WorkItemFaulted)]
        public async Task ShouldEmitFaultedWorkItemLifecycleEvents(int eventCode) {
            var options = new BackgroundTaskServiceOptions() {
                AllowWorkItemRegistrationWhileStopped = true,
                Name = TestContext.TestName + "_" + eventCode
            };

            using (var svc = ActivatorUtilities.CreateInstance<DefaultBackgroundTaskService>(s_serviceProvider, options))
            using (var ctSource = new CancellationTokenSource(5000)) {
                var tcs = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
                ctSource.Token.Register(() => tcs.TrySetCanceled());

                using (var listener = new Listener(eventData => {
                    if (eventData.EventId == eventCode) {
                        tcs.TrySetResult(0);
                    }
                })) {
                    _ = svc.RunAsync(ctSource.Token);
                    svc.QueueBackgroundWorkItem(ct => { throw new NotImplementedException(); });
                    await tcs.Task;
                }
            }
        }


        private class Listener : EventListener {

            private readonly Action<EventWrittenEventArgs> _callback;


            public Listener(Action<EventWrittenEventArgs> callback) {
                _callback = callback;
                EnableEvents(BackgroundTaskService.EventSource, EventLevel.Verbose);
            }


            protected override void OnEventWritten(EventWrittenEventArgs eventData) {
                base.OnEventWritten(eventData);
                _callback.Invoke(eventData);
            }

        }

    }

}
