using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalMirage.Data.Abstractions;
using PortalMirage.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskModel = PortalMirage.Core.Models.TaskModel; // Specific alias to avoid conflict

namespace PortalMirage.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")] // Only Admins can define master tasks
    public class TasksController(ITaskRepository taskRepository) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var tasks = await taskRepository.GetAllAsync();
            return Ok(tasks);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Deactivate(int id)
        {
            await taskRepository.DeactivateAsync(id);
            return NoContent();
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TaskModel task)
        {
            // The UI will send the ShiftID and ScheduleType
            var createdTask = await taskRepository.CreateAsync(task);
            return Ok(createdTask);
        }
    }
}