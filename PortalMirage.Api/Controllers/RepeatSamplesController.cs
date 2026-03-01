using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalMirage.Core.Dtos;
using PortalMirage.Business.Abstractions;
using PortalMirage.Core.Models;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace PortalMirage.Api.Controllers
{
    [ApiController]
    [Route("api/repeatsamples")]
    [Authorize]
    public class RepeatSamplesController : ControllerBase
    {
        private readonly IRepeatSampleLogService _repeatSampleLogService;
        private readonly IUserService _userService;
        private readonly ILogger<RepeatSamplesController> _logger;

        public RepeatSamplesController(
            IRepeatSampleLogService repeatSampleLogService,
            IUserService userService,
            ILogger<RepeatSamplesController> logger)
        {
            _repeatSampleLogService = repeatSampleLogService;
            _userService = userService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<RepeatSampleResponse>> Create([FromBody] CreateRepeatSampleRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            _logger.LogInformation("Creating repeat sample log for patient {PatientName} by user {UserId}", 
                request.PatientName, userId);

            var logToCreate = new RepeatSampleLog
            {
                PatientIdCardNumber = request.PatientIdCardNumber,
                PatientName = request.PatientName,
                ReasonText = request.ReasonText,
                InformedPerson = request.InformedPerson,
                Department = request.Department,
                LoggedByUserID = userId
            };

            var newLog = await _repeatSampleLogService.CreateAsync(logToCreate);
            var user = await _userService.GetUserByIdAsync(userId);
            var response = MapToResponse(newLog, user?.FullName ?? "Unknown");

            _logger.LogInformation("Repeat sample log created with ID: {RepeatId}", newLog.RepeatID);
            return Ok(response);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RepeatSampleResponse>>> GetByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            _logger.LogInformation("Fetching repeat samples from {StartDate} to {EndDate}", startDate, endDate);
            var logs = await _repeatSampleLogService.GetByDateRangeAsync(startDate, endDate);
            var users = (await _userService.GetAllUsersAsync()).ToDictionary(u => u.UserID);

            var response = logs.Select(log => MapToResponse(log,
                users.TryGetValue(log.LoggedByUserID, out var user) ? user.FullName : "Unknown"));

            return Ok(response);
        }

        [HttpPut("{id}/deactivate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Deactivate(int id, [FromBody] DeactivateRepeatSampleRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            _logger.LogInformation("Deactivating repeat sample {RepeatId} by user {UserId}", id, userId);
            
            var success = await _repeatSampleLogService.DeactivateAsync(id, userId, request.Reason);
            if (!success)
            {
                _logger.LogWarning("Failed to deactivate repeat sample {RepeatId}", id);
                return NotFound("Log not found or already deactivated.");
            }
            return Ok("Log deactivated successfully.");
        }

        private static RepeatSampleResponse MapToResponse(RepeatSampleLog log, string username)
        {
            return new RepeatSampleResponse(
                log.RepeatID,
                log.PatientIdCardNumber,
                log.PatientName,
                log.ReasonText,
                log.InformedPerson,
                log.Department,
                log.LogDateTime,
                log.LoggedByUserID,
                username);
        }
    }
}