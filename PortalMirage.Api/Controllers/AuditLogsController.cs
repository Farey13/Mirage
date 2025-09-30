using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalMirage.Business.Abstractions;
using System;
using System.Threading.Tasks;

namespace PortalMirage.Api.Controllers
{
    [ApiController]
    [Route("api/auditlogs")]
    [Authorize(Roles = "Admin")]
    public class AuditLogsController(IAuditLogService auditLogService) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var logs = await auditLogService.GetByDateRangeAsync(startDate, endDate);
            return Ok(logs);
        }
    }
}