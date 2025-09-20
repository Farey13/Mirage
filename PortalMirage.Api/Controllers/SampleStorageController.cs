using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalMirage.Api.Dtos;
using PortalMirage.Business.Abstractions;
using PortalMirage.Core.Models;

namespace PortalMirage.Api.Controllers
{
    [ApiController]
    [Route("api/samplestorage")] // Note the custom route name
    [Authorize]
    public class SampleStorageController(ISampleStorageService sampleStorageService) : ControllerBase
    {
        [HttpPost]
        public async Task<ActionResult<SampleStorageResponse>> Create([FromBody] CreateSampleStorageRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var logToCreate = new SampleStorage
            {
                PatientSampleID = request.PatientSampleID,
                StoredByUserID = userId
            };

            var newLog = await sampleStorageService.CreateAsync(logToCreate);
            var response = MapToResponse(newLog);
            return Ok(response);
        }

        [HttpGet("pending")]
        public async Task<ActionResult<IEnumerable<SampleStorageResponse>>> GetPending([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var logs = await sampleStorageService.GetPendingByDateRangeAsync(startDate, endDate);
            var response = logs.Select(MapToResponse);
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

        // Helper method to map model to DTO
        private static SampleStorageResponse MapToResponse(SampleStorage log)
        {
            return new SampleStorageResponse(
                log.StorageID,
                log.PatientSampleID,
                log.StorageDateTime,
                log.StoredByUserID,
                log.IsTestDone,
                log.TestDoneDateTime,
                log.TestDoneByUserID);
        }
    }
}