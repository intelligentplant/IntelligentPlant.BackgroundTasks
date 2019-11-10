using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IntelligentPlant.BackgroundTasks.Tests {
    [TestClass]
    public class BackgroundTaskServiceTests {

        [TestMethod]
        public void BackgroundTaskServiceShouldEnqueueTask() {

            using (var svc = new DefaultBackgroundTaskService(null)) {
                svc.QueueBackgroundWorkItem(ct => { 
                    // No-op
                });

                Assert.AreEqual(1, svc.QueuedItemCount);
            }

        }

    }
}
