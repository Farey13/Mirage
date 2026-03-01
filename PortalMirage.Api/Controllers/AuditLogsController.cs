using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalMirage.Business.Abstractions;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace PortalMirage.Api.Controllers
{
    [ApiController]
    [Route("api/auditlogs")]
    [Authorize(Roles = "Admin")]
    public class AuditLogsController(
        IAuditLogService auditLogService,
        ILogger<AuditLogsController> logger) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            logger.LogInformation("Fetching audit logs from {StartDate} to {EndDate}", startDate, endDate);
            var logs = await auditLogService.GetByDateRangeAsync(startDate, endDate);
            return Ok(logs);
        }
    }
}