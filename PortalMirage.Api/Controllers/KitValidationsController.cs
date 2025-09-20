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
    public class KitValidationsController(IKitValidationService kitValidationService) : ControllerBase
    {
        [HttpPost]
        public async Task<ActionResult<KitValidationResponse>> Create([FromBody] CreateKitValidationRequest request)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("User ID not found in token.");
            }

            var logToCreate = new KitValidation
            {
                KitName = request.KitName,
                KitLotNumber = request.KitLotNumber,
                KitExpiryDate = request.KitExpiryDate,
                ValidationStatus = request.ValidationStatus,
                Comments = request.Comments,
                ValidatedByUserID = userId
            };

            var newLog = await kitValidationService.CreateAsync(logToCreate);

            var response = new KitValidationResponse(
                newLog.ValidationID,
                newLog.KitName,
                newLog.KitLotNumber,
                newLog.KitExpiryDate,
                newLog.ValidationStatus,
                newLog.Comments,
                newLog.ValidationDateTime,
                newLog.ValidatedByUserID);

            return Ok(response);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<KitValidationResponse>>> GetByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var logs = await kitValidationService.GetByDateRangeAsync(startDate, endDate);

            var response = logs.Select(log => new KitValidationResponse(
                log.ValidationID,
                log.KitName,
                log.KitLotNumber,
                log.KitExpiryDate,
                log.ValidationStatus,
                log.Comments,
                log.ValidationDateTime,
                log.ValidatedByUserID));

            return Ok(response);
        }
    }
}