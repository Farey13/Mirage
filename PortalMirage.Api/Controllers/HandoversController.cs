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
    public class HandoversController(IHandoverService handoverService) : ControllerBase
    {
        [HttpPost]
        public async Task<ActionResult<HandoverResponse>> Create([FromBody] CreateHandoverRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var handoverToCreate = new Handover
            {
                HandoverNotes = request.HandoverNotes,
                GivenByUserID = userId
            };

            var newHandover = await handoverService.CreateAsync(handoverToCreate);
            var response = MapToResponse(newHandover);
            return Ok(response);
        }

        [HttpGet("pending")]
        public async Task<ActionResult<IEnumerable<HandoverResponse>>> GetPending()
        {
            var handovers = await handoverService.GetPendingAsync();
            var response = handovers.Select(MapToResponse);
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

        private static HandoverResponse MapToResponse(Handover handover)
        {
            return new HandoverResponse(
                handover.HandoverID,
                handover.HandoverNotes,
                handover.GivenDateTime,
                handover.GivenByUserID,
                handover.IsReceived,
                handover.ReceivedDateTime,
                handover.ReceivedByUserID);
        }
    }
}