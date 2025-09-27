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

namespace PortalMirage.Api.Controllers
{
    [ApiController]
    [Route("api/repeatsamples")]
    [Authorize]
    public class RepeatSamplesController(
        IRepeatSampleLogService repeatSampleLogService,
        IUserService userService) : ControllerBase
    {
        [HttpPost]
        public async Task<ActionResult<RepeatSampleResponse>> Create([FromBody] CreateRepeatSampleRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var logToCreate = new RepeatSampleLog
            {
                PatientIdCardNumber = request.PatientIdCardNumber,
                PatientName = request.PatientName,
                ReasonText = request.ReasonText,
                InformedPerson = request.InformedPerson,
                Department = request.Department,
                LoggedByUserID = userId
            };

            var newLog = await repeatSampleLogService.CreateAsync(logToCreate);
            var user = await userService.GetUserByIdAsync(userId);
            var response = MapToResponse(newLog, user?.FullName ?? "Unknown");

            return Ok(response);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RepeatSampleResponse>>> GetByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var logs = await repeatSampleLogService.GetByDateRangeAsync(startDate, endDate);
            var users = (await userService.GetAllUsersAsync()).ToDictionary(u => u.UserID);

            var response = logs.Select(log => MapToResponse(log,
                users.TryGetValue(log.LoggedByUserID, out var user) ? user.FullName : "Unknown"));

            return Ok(response);
        }

        [HttpPut("{id}/deactivate")]
        [Authorize(Roles = "Admin")] // Only Admins can deactivate
        public async Task<IActionResult> Deactivate(int id, [FromBody] DeactivateRepeatSampleRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var success = await repeatSampleLogService.DeactivateAsync(id, userId, request.Reason);
            if (!success)
            {
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