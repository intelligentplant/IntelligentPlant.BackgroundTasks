using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

using IntelligentPlant.BackgroundTasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ExampleAspNetApp.Controllers {

    [RoutePrefix("api/tasks")]
    public class TasksController : ApiController {

        private readonly ILogger _logger = WebApiApplication.ServiceProvider.GetRequiredService<ILogger<TasksController>>();


        [HttpGet]
        [Route("create-task")]
        public async Task<string> CreateTask(CancellationToken cancellationToken) {
            var tcs = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);

            var taskId = WebApiApplication.BackgroundTaskService.QueueBackgroundWorkItem(
                ct => {
                    try {
                        _logger.LogInformation("[BACKGROUND TASK] Running background task");
                    }
                    finally {
                        tcs.TrySetResult(0);
                        _logger.LogInformation("[BACKGROUND TASK] Background task completed");
                    }
                },
                null,
                true,
                cancellationToken
            );

            await tcs.Task.ConfigureAwait(false);
            return taskId;
        }

    }
}
