using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IntelligentPlant.BackgroundTasks.ExampleApp.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase {

        private readonly IBackgroundTaskService _backgroundTaskService;

        private readonly ILogger<TasksController> _logger;

        private static readonly Random s_rnd = new Random();


        public TasksController(IBackgroundTaskService backgroundTaskService, ILogger<TasksController> logger) {
            _backgroundTaskService = backgroundTaskService;
            _logger = logger;
        }


        [HttpGet]
        [Route("create-task")]
        public async Task<string> CreateTask(CancellationToken cancellationToken) {
            var tcs = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);

            var taskId = _backgroundTaskService.QueueBackgroundWorkItem(
                ct => {
                    try {
                        _logger.LogInformation("[BACKGROUND TASK] Running background task");
                        if (s_rnd.Next(101) < 30) {
                            throw new Exception("Failed");
                        }
                    }
                   finally {
                        tcs.TrySetResult(0);
                    }
                },
                "Example task",
                cancellationToken
            );

            await tcs.Task.ConfigureAwait(false);
            return taskId;
        }
    }
}
