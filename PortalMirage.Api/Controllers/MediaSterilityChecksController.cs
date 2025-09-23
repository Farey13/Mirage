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
    public class MediaSterilityChecksController(IMediaSterilityCheckService sterilityCheckService) : ControllerBase
    {
        [HttpPost]
        public async Task<ActionResult<MediaSterilityCheckResponse>> Create([FromBody] CreateMediaSterilityCheckRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var logToCreate = new MediaSterilityCheck
            {
                MediaName = request.MediaName,
                MediaLotNumber = request.MediaLotNumber,
                MediaQuantity = request.MediaQuantity,
                Result37C = request.Result37C,
                Result25C = request.Result25C,
                Comments = request.Comments,
                PerformedByUserID = userId,
                OverallStatus = "" // BLL will calculate this
            };

            var newLog = await sterilityCheckService.CreateAsync(logToCreate);
            var response = MapToResponse(newLog);
            return Ok(response);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MediaSterilityCheckResponse>>> GetByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var logs = await sterilityCheckService.GetByDateRangeAsync(startDate, endDate);
            var response = logs.Select(MapToResponse);
            return Ok(response);
        }

        private static MediaSterilityCheckResponse MapToResponse(MediaSterilityCheck log)
        {
            return new MediaSterilityCheckResponse(
                log.SterilityCheckID,
                log.MediaName,
                log.MediaLotNumber,
                log.MediaQuantity,
                log.Result37C,
                log.Result25C,
                log.OverallStatus,
                log.Comments,
                log.CheckDateTime,
                log.PerformedByUserID);
        }
    }
}