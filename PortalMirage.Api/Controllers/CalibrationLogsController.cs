using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalMirage.Business.Abstractions;
using PortalMirage.Core.Models;
using System.Linq;
using PortalMirage.Core.Dtos;

namespace PortalMirage.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CalibrationLogsController(ICalibrationLogService calibrationLogService, IUserService userService) : ControllerBase
    {
        // ... CreateLog method remains the same ...

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CalibrationLogResponse>>> GetLogsByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var logs = await calibrationLogService.GetByDateRangeAsync(startDate, endDate);
            var users = (await userService.GetAllUsersAsync()).ToDictionary(u => u.UserID);

            var response = logs.Select(log => new CalibrationLogResponse(
                log.CalibrationID,
                log.TestName,
                log.QcResult,
                log.Reason,
                log.CalibrationDateTime,
                log.PerformedByUserID,
                users.TryGetValue(log.PerformedByUserID, out var user) ? user.FullName : "Unknown"
            ));

            return Ok(response);
        }

        [HttpPut("{id}/deactivate")]
        public async Task<IActionResult> Deactivate(int id, [FromBody] DeactivateCalibrationLogRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var success = await calibrationLogService.DeactivateAsync(id, userId, request.Reason);
            if (!success)
            {
                return NotFound("Log not found or already deactivated.");
            }
            return Ok("Log deactivated successfully.");
        }
    }
}