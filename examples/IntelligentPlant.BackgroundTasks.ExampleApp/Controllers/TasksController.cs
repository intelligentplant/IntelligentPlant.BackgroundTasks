using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IntelligentPlant.BackgroundTasks.ExampleApp.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase {

        private readonly IBackgroundTaskService _backgroundTaskService;

        private readonly ILogger<TasksController> _logger;


        public TasksController(IBackgroundTaskService backgroundTaskService, ILogger<TasksController> logger) {
            _backgroundTaskService = backgroundTaskService;
            _logger = logger;
        }


        [HttpGet]
        [Route("create-task")]
        public Guid CreateTask() {
            return _backgroundTaskService.QueueBackgroundWorkItem(
                ct => _logger.LogInformation("[BACKGROUND TASK] Running background task"),
                "Example task"
            );
        }
    }
}
