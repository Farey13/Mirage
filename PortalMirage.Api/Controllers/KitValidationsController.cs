using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalMirage.Business.Abstractions;
using PortalMirage.Core.Models;
using System.Linq;
using PortalMirage.Core.Dtos;

namespace PortalMirage.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class KitValidationsController(IKitValidationService kitValidationService, IUserService userService) : ControllerBase
    {
        // ... Create method remains the same ...

        [HttpGet]
        public async Task<ActionResult<IEnumerable<KitValidationResponse>>> GetByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var logs = await kitValidationService.GetByDateRangeAsync(startDate, endDate);
            var users = (await userService.GetAllUsersAsync()).ToDictionary(u => u.UserID);

            var response = logs.Select(log => new KitValidationResponse(
                log.ValidationID,
                log.KitName,
                log.KitLotNumber,
                log.KitExpiryDate,
                log.ValidationStatus,
                log.Comments,
                log.ValidationDateTime,
                log.ValidatedByUserID,
                users.TryGetValue(log.ValidatedByUserID, out var user) ? user.FullName : "Unknown"
            ));

            return Ok(response);
        }
    }
}