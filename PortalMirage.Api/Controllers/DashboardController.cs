using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalMirage.Business.Abstractions;
using System.Threading.Tasks;

namespace PortalMirage.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController(IDashboardService dashboardService) : ControllerBase
    {
        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            var summary = await dashboardService.GetSummaryAsync();
            return Ok(summary);
        }
    }
}