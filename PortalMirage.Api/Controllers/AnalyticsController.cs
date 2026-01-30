using Microsoft.AspNetCore.Mvc;
using PortalMirage.Business.Abstractions;
using System;
using System.Threading.Tasks;

namespace PortalMirage.Api.Controllers;

[ApiController]
[Route("api/analytics")]
// Note: We removed the [Authorize] here so all authenticated users can see stats
public class AnalyticsController(IAnalyticsService analyticsService) : ControllerBase
{
    [HttpGet("daily-tasks/completion")]
    public async Task<IActionResult> GetTaskCompletion([FromQuery] DateTime start, [FromQuery] DateTime end)
    {
        var data = await analyticsService.GetDailyTaskCompletionAsync(start, end);
        return Ok(data);
    }

    [HttpGet("breakdown/downtime")]
    public async Task<IActionResult> GetMachineDowntime([FromQuery] DateTime start, [FromQuery] DateTime end)
    {
        var data = await analyticsService.GetMachineDowntimeAsync(start, end);
        return Ok(data);
    }
}