using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#if NETCOREAPP
using System.Linq;

using IntelligentPlant.BackgroundTasks.AspNetCore;

using Microsoft.Extensions.Hosting;
#endif

namespace IntelligentPlant.BackgroundTasks.Tests {
    [TestClass]
    public class BackgroundTaskServiceConfigurationTests {

        public TestContext TestContext { get; set; } = default!;


        [TestMethod]
        public void DefaultBackgroundTaskServiceShouldBeRegistered() {
            var services = new ServiceCollection();
            services.AddDefaultBackgroundTaskService();

            var serviceProvider = services.BuildServiceProvider();
            var btSvc = serviceProvider.GetService<IBackgroundTaskService>();

            Assert.IsNotNull(btSvc);
            Assert.AreEqual(BackgroundTaskService.Default, btSvc);
        }


        [TestMethod]
        public void BackgroundTaskServiceInstanceShouldBeRegistered() {
            var services = new ServiceCollection();

            var expected = new DefaultBackgroundTaskService(null, null);
            services.AddBackgroundTaskService(expected);

            var serviceProvider = services.BuildServiceProvider();
            var btSvc = serviceProvider.GetService<IBackgroundTaskService>();

            Assert.IsNotNull(btSvc);
            Assert.AreEqual(expected, btSvc);
        }


        [TestMethod]
        public void CustomBackgroundTaskServiceShouldBeRegistered() {
            var services = new ServiceCollection();
            services.AddBackgroundTaskService<TestBackgroundTaskService>();

            var serviceProvider = services.BuildServiceProvider();
            var btSvc = serviceProvider.GetService<IBackgroundTaskService>();

            Assert.IsNotNull(btSvc);
            Assert.IsTrue(btSvc is TestBackgroundTaskService);
        }


        [TestMethod]
        public void CustomOptionsShouldBePassedToBackgroundTaskService() {
            var services = new ServiceCollection();

            var onCompletedCalled = false;
            var onDequeuedCalled = false;
            var onEnqueuedCalled = false;
            var onErrorCalled = false;
            var onRunningCalled = false;

            services.AddBackgroundTaskService<TestBackgroundTaskService>(options => {
                options.AllowWorkItemRegistrationWhileStopped = true;
                options.OnCompleted = item => onCompletedCalled = true;
                options.OnDequeued = item => onDequeuedCalled = true;
                options.OnEnqueued = item => onEnqueuedCalled = true;
                options.OnError = (item, err) => onErrorCalled = true;
                options.OnRunning = item => onRunningCalled = true;
            });

            var serviceProvider = services.BuildServiceProvider();
            var btSvc = serviceProvider.GetService<IBackgroundTaskService>();

            Assert.IsNotNull(btSvc);
            var testSvc = btSvc as TestBackgroundTaskService;
            Assert.IsNotNull(testSvc);

            testSvc!.Options?.OnCompleted?.Invoke(default);
            Assert.IsTrue(onCompletedCalled);

            testSvc!.Options?.OnDequeued?.Invoke(default);
            Assert.IsTrue(onDequeuedCalled);

            testSvc!.Options?.OnEnqueued?.Invoke(default);
            Assert.IsTrue(onEnqueuedCalled);

            testSvc!.Options?.OnError?.Invoke(default, null!);
            Assert.IsTrue(onErrorCalled);

            testSvc!.Options?.OnRunning?.Invoke(default);
            Assert.IsTrue(onRunningCalled);
        }

#if NETCOREAPP

        [TestMethod]
        public void AspNetCoreBackgroundTaskServiceShouldBeRegistered() {
            var defaultSvc = BackgroundTaskService.Default;
            try {
                var services = new ServiceCollection();
                services.AddLogging();
                services.AddAspNetCoreBackgroundTaskService();

                var serviceProvider = services.BuildServiceProvider();
                var btSvc = serviceProvider.GetService<IBackgroundTaskService>();

                Assert.IsNotNull(btSvc);
                Assert.IsTrue(btSvc is AspNetCoreBackgroundTaskService);

                var hostedServices = serviceProvider.GetServices<IHostedService>();
                Assert.IsNotNull(hostedServices);

                var svcInitialiser = hostedServices.FirstOrDefault(x => x is AspNetCoreBackgroundTaskServiceRunner);
                Assert.IsNotNull(svcInitialiser);
            }
            finally {
                // Need to reset the default service so that we don't interfere with other tests.
                BackgroundTaskService.Default = defaultSvc;
            }
        }

#endif


        public class TestBackgroundTaskService : DefaultBackgroundTaskService {

            public BackgroundTaskServiceOptions Options { get; }

            public TestBackgroundTaskService(BackgroundTaskServiceOptions options) : base(options, null) {
                Options = options;
            }

        }

    }
}
