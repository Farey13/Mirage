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
    [Route("api/samplestorage")]
    [Authorize]
    public class SampleStorageController(ISampleStorageService sampleStorageService, IUserService userService) : ControllerBase
    {
        [HttpPost]
        public async Task<ActionResult<SampleStorageResponse>> Create([FromBody] CreateSampleStorageRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var logToCreate = new SampleStorage { PatientSampleID = request.PatientSampleID, TestName = request.TestName, StoredByUserID = userId };
            var newLog = await sampleStorageService.CreateAsync(logToCreate);
            var user = await userService.GetUserByIdAsync(userId);

            // This is the corrected line
            var response = MapToResponse(newLog, user?.FullName ?? "Unknown", null);

            return Ok(response);
        }

        [HttpGet("pending")]
        public async Task<ActionResult<IEnumerable<SampleStorageResponse>>> GetPending([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var logs = await sampleStorageService.GetPendingByDateRangeAsync(startDate, endDate);
            var users = (await userService.GetAllUsersAsync()).ToDictionary(u => u.UserID);

            // This is the corrected line
            var response = logs.Select(log => MapToResponse(log,
                users.TryGetValue(log.StoredByUserID, out var user) ? user.FullName : "Unknown",
                null));

            return Ok(response);
        }

        [HttpGet("completed")]
        public async Task<ActionResult<IEnumerable<SampleStorageResponse>>> GetCompleted([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var logs = await sampleStorageService.GetCompletedByDateRangeAsync(startDate, endDate);
            var users = (await userService.GetAllUsersAsync()).ToDictionary(u => u.UserID);

            var response = logs.Select(log => MapToResponse(
                log,
                users.TryGetValue(log.StoredByUserID, out var storedByUser) ? storedByUser.FullName : "Unknown",
                log.TestDoneByUserID.HasValue && users.TryGetValue(log.TestDoneByUserID.Value, out var doneByUser) ? doneByUser.FullName : null
                ));

            return Ok(response);
        }

        [HttpPut("{id}/done")]
        public async Task<IActionResult> MarkAsDone(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var success = await sampleStorageService.MarkAsDoneAsync(id, userId);

            if (!success)
            {
                return NotFound("Sample not found or already completed.");
            }

            return Ok("Sample marked as done.");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Deactivate(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var success = await sampleStorageService.DeactivateAsync(id, userId);
            if (!success)
            {
                return NotFound();
            }
            return Ok("Sample entry deactivated successfully.");
        }

        private static SampleStorageResponse MapToResponse(SampleStorage log, string storedByUsername, string? doneByUsername)
        {
            return new SampleStorageResponse(
                log.StorageID,
                log.PatientSampleID,
                log.TestName,
                log.StorageDateTime,
                log.StoredByUserID,
                storedByUsername,
                log.IsTestDone,
                log.TestDoneDateTime,
                log.TestDoneByUserID,
                doneByUsername);
        }
    }
}