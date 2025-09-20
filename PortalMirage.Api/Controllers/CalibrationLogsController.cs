using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalMirage.Api.Dtos;
using PortalMirage.Business.Abstractions;
using PortalMirage.Core.Models;

namespace PortalMirage.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CalibrationLogsController(ICalibrationLogService calibrationLogService) : ControllerBase
    {
        [HttpPost]
        public async Task<ActionResult<CalibrationLogResponse>> CreateLog([FromBody] CreateCalibrationLogRequest request)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("User ID not found in token.");
            }

            var logToCreate = new CalibrationLog
            {
                TestName = request.TestName,
                QcResult = request.QcResult,
                Reason = request.Reason,
                PerformedByUserID = userId
            };

            var newLog = await calibrationLogService.CreateAsync(logToCreate);

            var response = new CalibrationLogResponse(
                newLog.CalibrationID,
                newLog.TestName,
                newLog.QcResult,
                newLog.Reason,
                newLog.CalibrationDateTime,
                newLog.PerformedByUserID);

            return Ok(response);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CalibrationLogResponse>>> GetLogsByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var logs = await calibrationLogService.GetByDateRangeAsync(startDate, endDate);

            var response = logs.Select(log => new CalibrationLogResponse(
                log.CalibrationID,
                log.TestName,
                log.QcResult,
                log.Reason,
                log.CalibrationDateTime,
                log.PerformedByUserID));

            return Ok(response);
        }
    }
}