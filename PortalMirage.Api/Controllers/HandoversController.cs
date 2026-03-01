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
    public class HandoversController(
        IHandoverService handoverService, 
        IUserService userService,
        ILogger<HandoversController> logger) : ControllerBase
    {
        [HttpPost]
        public async Task<ActionResult<HandoverResponse>> Create([FromBody] CreateHandoverRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            logger.LogInformation("Creating handover by user {UserId}, Priority: {Priority}, Shift: {Shift}", 
                userId, request.Priority, request.Shift);
            
            var handoverToCreate = new Handover
            {
                HandoverNotes = request.HandoverNotes,
                Priority = request.Priority,
                Shift = request.Shift,
                GivenByUserID = userId
            };

            var newHandover = await handoverService.CreateAsync(handoverToCreate);
            var user = await userService.GetUserByIdAsync(userId);
            var response = MapToResponse(newHandover, user?.FullName ?? "Unknown", null);
            
            logger.LogInformation("Handover created with ID: {HandoverId}", newHandover.HandoverID);
            return Ok(response);
        }

        [HttpGet("pending")]
        public async Task<ActionResult<IEnumerable<HandoverResponse>>> GetPending([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            logger.LogInformation("Fetching pending handovers from {StartDate} to {EndDate}", startDate, endDate);
            var handovers = await handoverService.GetPendingAsync(startDate, endDate);
            var users = (await userService.GetAllUsersAsync()).ToDictionary(u => u.UserID);

            var response = handovers.Select(h => MapToResponse(h,
                users.TryGetValue(h.GivenByUserID, out var givenByUser) ? givenByUser.FullName : "Unknown",
                null
            ));
            return Ok(response);
        }

        [HttpGet("completed")]
        public async Task<ActionResult<IEnumerable<HandoverResponse>>> GetCompleted([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            logger.LogInformation("Fetching completed handovers from {StartDate} to {EndDate}", startDate, endDate);
            var handovers = await handoverService.GetCompletedAsync(startDate, endDate);
            var users = (await userService.GetAllUsersAsync()).ToDictionary(u => u.UserID);

            var response = handovers.Select(h => MapToResponse(h,
                users.TryGetValue(h.GivenByUserID, out var givenByUser) ? givenByUser.FullName : "Unknown",
                h.ReceivedByUserID.HasValue && users.TryGetValue(h.ReceivedByUserID.Value, out var receivedByUser) ? receivedByUser.FullName : null
            ));
            return Ok(response);
        }

        [HttpPut("{id}/receive")]
        public async Task<IActionResult> MarkAsReceived(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            logger.LogInformation("Marking handover as received: {HandoverId} by user {UserId}", id, userId);
            
            var success = await handoverService.MarkAsReceivedAsync(id, userId);

            if (!success)
            {
                logger.LogWarning("Failed to mark handover as received: {HandoverId}", id);
                return NotFound("Handover not found or already received.");
            }
            return Ok("Handover marked as received.");
        }

        [HttpPut("{id}/deactivate")]
        public async Task<IActionResult> Deactivate(int id, [FromBody] DeactivateHandoverRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var handover = await handoverService.GetByIdAsync(id);

            if (handover is null) 
            {
                logger.LogWarning("Handover not found: {HandoverId}", id);
                return NotFound();
            }

            if (handover.IsReceived && !User.IsInRole("Admin"))
            {
                logger.LogWarning("User {UserId} attempted to deactivate completed handover without Admin role", userId);
                return Forbid("Only Admins can deactivate a completed handover.");
            }

            logger.LogInformation("Deactivating handover {HandoverId} by user {UserId}", id, userId);
            var success = await handoverService.DeactivateAsync(id, userId, request.Reason);
            if (!success)
            {
                logger.LogWarning("Failed to deactivate handover {HandoverId}", id);
                return NotFound("Handover not found or already deactivated.");
            }
            return Ok("Handover deactivated successfully.");
        }

        private static HandoverResponse MapToResponse(Handover h, string givenBy, string? receivedBy)
        {
            return new HandoverResponse(h.HandoverID, h.HandoverNotes, h.Priority, h.Shift, h.GivenDateTime, h.GivenByUserID, givenBy, h.IsReceived, h.ReceivedDateTime, h.ReceivedByUserID, receivedBy);
        }
    }
}