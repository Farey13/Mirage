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
        [HttpGet("kit-validations")] // ADD THIS NEW ENDPOINT
        public async Task<IActionResult> GetKitValidationReport(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] string? kitName,
        [FromQuery] string? status)
        {
            var reportData = await reportService.GetKitValidationReportAsync(startDate, endDate, kitName, status);
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

        [HttpGet("media-sterility")] // ADD THIS NEW ENDPOINT
        public async Task<IActionResult> GetMediaSterilityReport(
    [FromQuery] DateTime startDate,
    [FromQuery] DateTime endDate,
    [FromQuery] string? mediaName,
    [FromQuery] string? status)
        {
            var reportData = await reportService.GetMediaSterilityReportAsync(startDate, endDate, mediaName, status);
            return Ok(reportData);
        }

        [HttpGet("sample-storage")] // ADD THIS NEW ENDPOINT
        public async Task<IActionResult> GetSampleStorageReport(
    [FromQuery] DateTime startDate,
    [FromQuery] DateTime endDate,
    [FromQuery] string? testName,
    [FromQuery] string? status)
        {
            var reportData = await reportService.GetSampleStorageReportAsync(startDate, endDate, testName, status);
            return Ok(reportData);
        }

        [HttpGet("repeat-samples")] // ADD THIS NEW ENDPOINT
        public async Task<IActionResult> GetRepeatSampleReport(
             [FromQuery] DateTime startDate,
             [FromQuery] DateTime endDate,
             [FromQuery] string? reason,
             [FromQuery] string? department)
        {
            var reportData = await reportService.GetRepeatSampleReportAsync(startDate, endDate, reason, department);
            return Ok(reportData);
        }
        [HttpGet("daily-task-compliance")] // ADD THIS NEW ENDPOINT
        public async Task<IActionResult> GetDailyTaskComplianceReport(
    [FromQuery] DateTime startDate,
    [FromQuery] DateTime endDate,
    [FromQuery] int? shiftId,
    [FromQuery] string? status)
        {
            var reportData = await reportService.GetDailyTaskComplianceReportAsync(startDate, endDate, shiftId, status);
            return Ok(reportData);
        }
    }
}