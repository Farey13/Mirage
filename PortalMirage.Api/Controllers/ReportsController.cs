using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalMirage.Business.Abstractions;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace PortalMirage.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReportsController(
        IReportService reportService,
        ILogger<ReportsController> logger) : ControllerBase
    {
        [HttpGet("machine-breakdowns")]
        public async Task<IActionResult> GetMachineBreakdownReport(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] string? machineName,
            [FromQuery] string? status)
        {
            logger.LogInformation("Generating machine breakdown report from {StartDate} to {EndDate}", startDate, endDate);
            var reportData = await reportService.GetMachineBreakdownReportAsync(startDate, endDate, machineName, status);
            return Ok(reportData);
        }
        
        [HttpGet("kit-validations")]
        public async Task<IActionResult> GetKitValidationReport(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] string? kitName,
            [FromQuery] string? status)
        {
            logger.LogInformation("Generating kit validation report from {StartDate} to {EndDate}", startDate, endDate);
            var reportData = await reportService.GetKitValidationReportAsync(startDate, endDate, kitName, status);
            return Ok(reportData);
        }

        [HttpGet("handovers")]
        public async Task<IActionResult> GetHandoverReport(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] string? shift,
            [FromQuery] string? priority,
            [FromQuery] string? status)
        {
            logger.LogInformation("Generating handover report from {StartDate} to {EndDate}", startDate, endDate);
            var reportData = await reportService.GetHandoverReportAsync(startDate, endDate, shift, priority, status);
            return Ok(reportData);
        }

        [HttpGet("media-sterility")]
        public async Task<IActionResult> GetMediaSterilityReport(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] string? mediaName,
            [FromQuery] string? status)
        {
            logger.LogInformation("Generating media sterility report from {StartDate} to {EndDate}", startDate, endDate);
            var reportData = await reportService.GetMediaSterilityReportAsync(startDate, endDate, mediaName, status);
            return Ok(reportData);
        }

        [HttpGet("calibrations")]
        public async Task<IActionResult> GetCalibrationReport(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] string? testName,
            [FromQuery] string? qcResult)
        {
            logger.LogInformation("Generating calibration report from {StartDate} to {EndDate}", startDate, endDate);
            var reportData = await reportService.GetCalibrationReportAsync(startDate, endDate, testName, qcResult);
            return Ok(reportData);
        }

        [HttpGet("sample-storage")]
        public async Task<IActionResult> GetSampleStorageReport(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] string? testName,
            [FromQuery] string? status)
        {
            logger.LogInformation("Generating sample storage report from {StartDate} to {EndDate}", startDate, endDate);
            var reportData = await reportService.GetSampleStorageReportAsync(startDate, endDate, testName, status);
            return Ok(reportData);
        }

        [HttpGet("repeat-samples")]
        public async Task<IActionResult> GetRepeatSampleReport(
             [FromQuery] DateTime startDate,
             [FromQuery] DateTime endDate,
             [FromQuery] string? reason,
             [FromQuery] string? department)
        {
            logger.LogInformation("Generating repeat samples report from {StartDate} to {EndDate}", startDate, endDate);
            var reportData = await reportService.GetRepeatSampleReportAsync(startDate, endDate, reason, department);
            return Ok(reportData);
        }
        
        [HttpGet("daily-task-compliance")]
        public async Task<IActionResult> GetDailyTaskComplianceReport(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] int? shiftId,
            [FromQuery] string? status)
        {
            logger.LogInformation("Generating daily task compliance report from {StartDate} to {EndDate}", startDate, endDate);
            var reportData = await reportService.GetDailyTaskComplianceReportAsync(startDate, endDate, shiftId, status);
            return Ok(reportData);
        }
    }
}