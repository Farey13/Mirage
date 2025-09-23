using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalMirage.Core.Dtos; // This is the new, correct location
using PortalMirage.Business.Abstractions;
using PortalMirage.Core.Models;

namespace PortalMirage.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MachineBreakdownsController(IMachineBreakdownService machineBreakdownService) : ControllerBase
    {
        [HttpPost]
        public async Task<ActionResult<MachineBreakdownResponse>> Create([FromBody] CreateMachineBreakdownRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var breakdownToCreate = new MachineBreakdown
            {
                MachineName = request.MachineName,
                BreakdownReason = request.BreakdownReason,
                ReportedByUserID = userId
            };

            var newBreakdown = await machineBreakdownService.CreateAsync(breakdownToCreate);
            var response = MapToResponse(newBreakdown);
            return Ok(response);
        }

        [HttpGet("pending")]
        public async Task<ActionResult<IEnumerable<MachineBreakdownResponse>>> GetPending([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var breakdowns = await machineBreakdownService.GetPendingByDateRangeAsync(startDate, endDate);
            var response = breakdowns.Select(MapToResponse);
            return Ok(response);
        }

        [HttpPut("{id}/resolve")]
        public async Task<IActionResult> MarkAsResolved(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var success = await machineBreakdownService.MarkAsResolvedAsync(id, userId);

            if (!success)
            {
                return NotFound("Breakdown not found or already resolved.");
            }

            return Ok("Breakdown marked as resolved.");
        }

        private static MachineBreakdownResponse MapToResponse(MachineBreakdown breakdown)
        {
            return new MachineBreakdownResponse(
                breakdown.BreakdownID,
                breakdown.MachineName,
                breakdown.BreakdownReason,
                breakdown.ReportedDateTime,
                breakdown.ReportedByUserID,
                breakdown.IsResolved,
                breakdown.ResolvedDateTime,
                breakdown.ResolvedByUserID);
        }
    }
}