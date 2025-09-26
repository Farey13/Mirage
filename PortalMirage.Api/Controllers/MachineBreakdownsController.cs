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
    public class MachineBreakdownsController(IMachineBreakdownService machineBreakdownService, IUserService userService) : ControllerBase
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
            var user = await userService.GetUserByIdAsync(userId);
            var response = MapToResponse(newBreakdown, user?.FullName ?? "Unknown", null);
            return Ok(response);
        }

        [HttpGet("pending")]
        public async Task<ActionResult<IEnumerable<MachineBreakdownResponse>>> GetPending([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var breakdowns = await machineBreakdownService.GetPendingByDateRangeAsync(startDate, endDate);
            var users = (await userService.GetAllUsersAsync()).ToDictionary(u => u.UserID);
            var response = breakdowns.Select(b => MapToResponse(b,
                users.TryGetValue(b.ReportedByUserID, out var reportedByUser) ? reportedByUser.FullName : "Unknown",
                null));
            return Ok(response);
        }

        [HttpGet("resolved")]
        public async Task<ActionResult<IEnumerable<MachineBreakdownResponse>>> GetResolved([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var breakdowns = await machineBreakdownService.GetResolvedByDateRangeAsync(startDate, endDate);
            var users = (await userService.GetAllUsersAsync()).ToDictionary(u => u.UserID);

            var response = breakdowns.Select(b =>
            {
                var reportedBy = users.TryGetValue(b.ReportedByUserID, out var rbu) ? rbu.FullName : "Unknown";

                string? resolvedBy = null;
                if (b.ResolvedByUserID.HasValue && users.TryGetValue(b.ResolvedByUserID.Value, out var rsu))
                {
                    resolvedBy = rsu.FullName;
                }

                return MapToResponse(b, reportedBy, resolvedBy);
            });

            return Ok(response);
        }

        [HttpPut("{id}/deactivate")]
        public async Task<IActionResult> Deactivate(int id, [FromBody] DeactivateMachineBreakdownRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var breakdown = await machineBreakdownService.GetByIdAsync(id);

            if (breakdown is null) return NotFound();

            // Business Rule: If breakdown is already resolved, only an Admin can deactivate it
            if (breakdown.IsResolved && !User.IsInRole("Admin"))
            {
                return Forbid("Only Admins can deactivate a resolved issue.");
            }

            var success = await machineBreakdownService.DeactivateAsync(id, userId, request.Reason);
            if (!success)
            {
                return NotFound("Breakdown not found or already deactivated.");
            }
            return Ok("Breakdown deactivated successfully.");
        }

        [HttpPut("{id}/resolve")]
        public async Task<IActionResult> MarkAsResolved(int id, [FromBody] ResolveBreakdownRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var success = await machineBreakdownService.MarkAsResolvedAsync(id, userId, request.ResolutionNotes);

            if (!success)
            {
                return NotFound("Breakdown not found or already resolved.");
            }

            return Ok("Breakdown marked as resolved.");
        }

        private static MachineBreakdownResponse MapToResponse(MachineBreakdown breakdown, string reportedByUsername, string? resolvedByUsername)
        {
            return new MachineBreakdownResponse(
                breakdown.BreakdownID, breakdown.MachineName, breakdown.BreakdownReason,
                breakdown.ReportedDateTime, breakdown.ReportedByUserID, reportedByUsername,
                breakdown.IsResolved, breakdown.ResolvedDateTime, breakdown.ResolvedByUserID,
                resolvedByUsername, breakdown.ResolutionNotes, breakdown.DowntimeMinutes);
        }
    }
}