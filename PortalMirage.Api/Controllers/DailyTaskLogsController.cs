using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalMirage.Core.Dtos;
using PortalMirage.Business.Abstractions;

namespace PortalMirage.Api.Controllers
{
    [ApiController]
    [Route("api/dailytasklogs")]
    [Authorize]
    public class DailyTaskLogsController(IDailyTaskLogService dailyTaskLogService) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetForDate([FromQuery] DateTime date)
        {
            var tasks = await dailyTaskLogService.GetTasksForDateAsync(date);
            return Ok(tasks);
        }

        [HttpPut("{id}/extend")]
        [Authorize(Roles = "Admin")] // Only Admins can extend deadlines
        public async Task<IActionResult> ExtendDeadline(long id, [FromBody] ExtendTaskDeadlineRequest request)
        {
            var adminUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var updatedLog = await dailyTaskLogService.ExtendTaskDeadlineAsync(id, request.NewDeadline, request.Reason, adminUserId);

            if (updatedLog is null)
            {
                return NotFound("Task log not found or could not be extended.");
            }

            return Ok(updatedLog);
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(long id, [FromBody] UpdateTaskStatusRequest request)
        {
            // --- CRASH PROOF WRAPPER ---
            try
            {
                // 1. TRUST THE CLIENT: Use the ID sent from the App
                int userId = request.UserId;

                // 2. FALLBACK: Only look at token if Client sent 0
                if (userId <= 0)
                {
                    // Safe lookup that won't crash
                    var claimId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                                  ?? User.FindFirstValue("sub")
                                  ?? User.FindFirstValue("id");

                    if (!string.IsNullOrEmpty(claimId))
                    {
                        int.TryParse(claimId, out userId);
                    }
                }

                // 3. FINAL VALIDATION
                if (userId <= 0)
                {
                    // This message will appear in your App if ID is missing
                    return BadRequest("Server Error: User ID could not be identified from Request or Token.");
                }

                // 4. EXECUTE SERVICE
                var updatedTaskLog = await dailyTaskLogService.UpdateTaskStatusAsync(id, request.Status, userId, request.Comment);

                if (updatedTaskLog is null)
                {
                    return NotFound($"Task log with ID {id} not found.");
                }

                return Ok(updatedTaskLog);
            }
            catch (Exception ex)
            {
                // 5. CATCH THE ERROR: Return the specific error message instead of '500'
                return StatusCode(500, $"CRITICAL FAILURE: {ex.Message} \n\n Stack Trace: {ex.StackTrace}");
            }
        }
    }
}