using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalMirage.Core.Dtos; // This is the new, correct location
using PortalMirage.Business.Abstractions;

namespace PortalMirage.Api.Controllers
{
    [ApiController]
    [Route("api/dailytasklogs")]
    [Authorize]
    public class DailyTaskLogsController(IDailyTaskLogService dailyTaskLogService) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetForDate([FromQuery] DateTime date) // Changed from DateOnly
        {
            var tasks = await dailyTaskLogService.GetTasksForDateAsync(date);
            return Ok(tasks);
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(long id, [FromBody] UpdateTaskStatusRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var updatedTaskLog = await dailyTaskLogService.UpdateTaskStatusAsync(id, request.Status, userId);

            if (updatedTaskLog is null)
            {
                return NotFound("Task log not found.");
            }

            return Ok(updatedTaskLog);
        }
    }
}