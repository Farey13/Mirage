using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalMirage.Core.Dtos;
using PortalMirage.Business.Abstractions;
using PortalMirage.Core.Models;
using Microsoft.Extensions.Logging;

namespace PortalMirage.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MediaSterilityChecksController(
        IMediaSterilityCheckService sterilityCheckService,
        IUserService userService,
        ILogger<MediaSterilityChecksController> logger) : ControllerBase
    {
        [HttpPost]
        public async Task<ActionResult<MediaSterilityCheckResponse>> Create([FromBody] CreateMediaSterilityCheckRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            logger.LogInformation("Creating media sterility check for {MediaName}, Lot: {MediaLotNumber} by user {UserId}", 
                request.MediaName, request.MediaLotNumber, userId);
            
            var logToCreate = new MediaSterilityCheck
            {
                MediaName = request.MediaName,
                MediaLotNumber = request.MediaLotNumber,
                MediaQuantity = request.MediaQuantity,
                Result37C = request.Result37C,
                Result25C = request.Result25C,
                Comments = request.Comments,
                PerformedByUserID = userId,
                OverallStatus = ""
            };

            var newLog = await sterilityCheckService.CreateAsync(logToCreate);
            var user = await userService.GetUserByIdAsync(userId);
            var response = MapToResponse(newLog, user?.FullName ?? "Unknown");
            
            logger.LogInformation("Media sterility check created with ID: {SterilityCheckId}", newLog.SterilityCheckID);
            return Ok(response);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MediaSterilityCheckResponse>>> GetByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            logger.LogInformation("Fetching media sterility checks from {StartDate} to {EndDate}", startDate, endDate);
            var logs = await sterilityCheckService.GetByDateRangeAsync(startDate, endDate);
            var users = (await userService.GetAllUsersAsync()).ToDictionary(u => u.UserID);
            var response = logs.Select(log => MapToResponse(log,
                users.TryGetValue(log.PerformedByUserID, out var user) ? user.FullName : "Unknown"));
            return Ok(response);
        }

        [HttpPut("{id}/deactivate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Deactivate(int id, [FromBody] DeactivateMediaSterilityCheckRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            logger.LogInformation("Deactivating media sterility check {SterilityCheckId} by user {UserId}", id, userId);
            
            var success = await sterilityCheckService.DeactivateAsync(id, userId, request.Reason);
            if (!success)
            {
                logger.LogWarning("Failed to deactivate media sterility check {SterilityCheckId}", id);
                return NotFound("Log not found or already deactivated.");
            }
            return Ok("Log deactivated successfully.");
        }

        private static MediaSterilityCheckResponse MapToResponse(MediaSterilityCheck log, string username)
        {
            return new MediaSterilityCheckResponse(
                log.SterilityCheckID, log.MediaName, log.MediaLotNumber, log.MediaQuantity,
                log.Result37C, log.Result25C, log.OverallStatus, log.Comments,
                log.CheckDateTime, log.PerformedByUserID, username);
        }
    }
}