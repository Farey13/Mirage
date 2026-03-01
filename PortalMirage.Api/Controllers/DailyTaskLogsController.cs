using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalMirage.Core.Dtos;
using PortalMirage.Business.Abstractions;
using Microsoft.Extensions.Logging;

namespace PortalMirage.Api.Controllers
{
    [ApiController]
    [Route("api/dailytasklogs")]
    [Authorize]
    public class DailyTaskLogsController(
        IDailyTaskLogService dailyTaskLogService,
        ILogger<DailyTaskLogsController> logger) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetForDate([FromQuery] DateTime date)
        {
            logger.LogInformation("Fetching daily task logs for date: {Date}", date);
            var tasks = await dailyTaskLogService.GetTasksForDateAsync(date);
            logger.LogInformation("Retrieved {Count} task logs for date: {Date}", tasks.Count(), date);
            return Ok(tasks);
        }

        [HttpPut("{id}/extend")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ExtendDeadline(long id, [FromBody] ExtendTaskDeadlineRequest request)
        {
            var adminUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            logger.LogInformation("Extending deadline for task log {TaskLogId} to {NewDeadline} by user {UserId}", 
                id, request.NewDeadline, adminUserId);
            
            var updatedLog = await dailyTaskLogService.ExtendTaskDeadlineAsync(id, request.NewDeadline, request.Reason, adminUserId);

            if (updatedLog is null)
            {
                logger.LogWarning("Failed to extend deadline for task log {TaskLogId}", id);
                return NotFound("Task log not found or could not be extended.");
            }

            return Ok(updatedLog);
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(long id, [FromBody] UpdateTaskStatusRequest request)
        {
            try
            {
                int userId = request.UserId;

                if (userId <= 0)
                {
                    var claimId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                                  ?? User.FindFirstValue("sub")
                                  ?? User.FindFirstValue("id");

                    if (!string.IsNullOrEmpty(claimId))
                    {
                        int.TryParse(claimId, out userId);
                    }
                }

                if (userId <= 0)
                {
                    return BadRequest("Server Error: User ID could not be identified from Request or Token.");
                }

                logger.LogInformation("Updating status for task log {TaskLogId} to {Status} by user {UserId}", 
                    id, request.Status, userId);
                
                var updatedTaskLog = await dailyTaskLogService.UpdateTaskStatusAsync(id, request.Status, userId, request.Comment);

                if (updatedTaskLog is null)
                {
                    logger.LogWarning("Task log not found: {TaskLogId}", id);
                    return NotFound($"Task log with ID {id} not found.");
                }

                return Ok(updatedTaskLog);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating task log status for {TaskLogId}", id);
                return StatusCode(500, $"CRITICAL FAILURE: {ex.Message} \n\n Stack Trace: {ex.StackTrace}");
            }
        }
    }
}