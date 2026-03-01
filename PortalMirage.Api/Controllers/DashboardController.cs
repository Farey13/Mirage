using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalMirage.Business.Abstractions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace PortalMirage.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController(
        IDashboardService dashboardService,
        ILogger<DashboardController> logger) : ControllerBase
    {
        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            logger.LogInformation("Fetching dashboard summary");
            var summary = await dashboardService.GetSummaryAsync();
            return Ok(summary);
        }
    }
}