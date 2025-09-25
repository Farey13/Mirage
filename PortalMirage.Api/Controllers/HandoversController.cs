using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalMirage.Core.Dtos;
using PortalMirage.Business.Abstractions;
using PortalMirage.Core.Models;

namespace PortalMirage.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class HandoversController(IHandoverService handoverService, IUserService userService) : ControllerBase
    {
        [HttpPost]
        public async Task<ActionResult<HandoverResponse>> Create([FromBody] CreateHandoverRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
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
            return Ok(response);
        }

        [HttpGet("pending")]
        public async Task<ActionResult<IEnumerable<HandoverResponse>>> GetPending()
        {
            var handovers = await handoverService.GetPendingAsync();
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
            var success = await handoverService.MarkAsReceivedAsync(id, userId);

            if (!success)
            {
                return NotFound("Handover not found or already received.");
            }
            return Ok("Handover marked as received.");
        }

        private static HandoverResponse MapToResponse(Handover h, string givenBy, string? receivedBy)
        {
            return new HandoverResponse(h.HandoverID, h.HandoverNotes, h.Priority, h.Shift, h.GivenDateTime, h.GivenByUserID, givenBy, h.IsReceived, h.ReceivedDateTime, h.ReceivedByUserID, receivedBy);
        }
    }
}