using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalMirage.Business.Abstractions;
using PortalMirage.Core.Models;
using System.Linq;
using PortalMirage.Core.Dtos;
using Microsoft.Extensions.Logging;

namespace PortalMirage.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CalibrationLogsController(
        ICalibrationLogService calibrationLogService, 
        IUserService userService,
        ILogger<CalibrationLogsController> logger) : ControllerBase
    {
        [HttpPost]
        public async Task<ActionResult<CalibrationLogResponse>> Create([FromBody] CreateCalibrationLogRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            logger.LogInformation("Creating calibration log for test: {TestName}, QcResult: {QcResult} by user {UserId}", 
                request.TestName, request.QcResult, userId);
            
            var logToCreate = new CalibrationLog
            {
                TestName = request.TestName,
                QcResult = request.QcResult,
                Reason = request.Reason,
                PerformedByUserID = userId
            };

            var newLog = await calibrationLogService.CreateAsync(logToCreate);
            var user = await userService.GetUserByIdAsync(userId);
            var response = new CalibrationLogResponse(
                newLog.CalibrationID,
                newLog.TestName,
                newLog.QcResult,
                newLog.Reason,
                newLog.CalibrationDateTime,
                newLog.PerformedByUserID,
                user?.FullName ?? "Unknown"
            );
            
            logger.LogInformation("Calibration log created with ID: {CalibrationId}", newLog.CalibrationID);
            return Ok(response);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CalibrationLogResponse>>> GetLogsByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            logger.LogInformation("Fetching calibration logs from {StartDate} to {EndDate}", startDate, endDate);
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
            logger.LogInformation("Deactivating calibration log {CalibrationId} by user {UserId}", id, userId);
            
            var success = await calibrationLogService.DeactivateAsync(id, userId, request.Reason);
            if (!success)
            {
                logger.LogWarning("Failed to deactivate calibration log {CalibrationId}", id);
                return NotFound("Log not found or already deactivated.");
            }
            return Ok("Log deactivated successfully.");
        }
    }
}