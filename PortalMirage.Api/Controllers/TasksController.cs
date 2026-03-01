using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalMirage.Data.Abstractions;
using PortalMirage.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TaskModel = PortalMirage.Core.Models.TaskModel;

namespace PortalMirage.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class TasksController(
        ITaskRepository taskRepository,
        ILogger<TasksController> logger) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            logger.LogInformation("Fetching all tasks");
            var tasks = await taskRepository.GetAllAsync();
            logger.LogInformation("Retrieved {Count} tasks", tasks.Count());
            return Ok(tasks);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Deactivate(int id)
        {
            logger.LogInformation("Deactivating task {TaskId}", id);
            await taskRepository.DeactivateAsync(id);
            return NoContent();
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TaskModel task)
        {
            logger.LogInformation("Creating new task: {TaskName}, ShiftId: {ShiftId}", task.TaskName, task.ShiftID);
            var createdTask = await taskRepository.CreateAsync(task);
            logger.LogInformation("Task created with ID: {TaskId}", createdTask.TaskID);
            return Ok(createdTask);
        }
    }
}