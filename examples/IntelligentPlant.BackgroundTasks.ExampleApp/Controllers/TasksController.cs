using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            var taskId = Guid.NewGuid();

            _backgroundTaskService.QueueBackgroundWorkItem(ct => _logger.LogInformation("Running background task {TaskId}", taskId));

            return taskId;
        }
    }
}
