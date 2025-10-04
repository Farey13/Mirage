using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalMirage.Business.Abstractions;
using System;
using System.Threading.Tasks;

namespace PortalMirage.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // All reports require the user to be logged in
    public class ReportsController(IReportService reportService) : ControllerBase
    {
        [HttpGet("machine-breakdowns")]
        public async Task<IActionResult> GetMachineBreakdownReport(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] string? machineName,
            [FromQuery] string? status)
        {
            var reportData = await reportService.GetMachineBreakdownReportAsync(startDate, endDate, machineName, status);
            return Ok(reportData);
        }

        [HttpGet("handovers")] // ADD THIS NEW ENDPOINT
        public async Task<IActionResult> GetHandoverReport(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] string? shift,
            [FromQuery] string? priority,
            [FromQuery] string? status)
        {
            var reportData = await reportService.GetHandoverReportAsync(startDate, endDate, shift, priority, status);
            return Ok(reportData);
        }
    }
}