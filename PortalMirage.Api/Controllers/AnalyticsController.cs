using Microsoft.AspNetCore.Mvc;
using PortalMirage.Business.Abstractions;
using PortalMirage.Core.Dtos;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace PortalMirage.Api.Controllers;

[ApiController]
[Route("api/analytics")]
public class AnalyticsController(
    IAnalyticsService analyticsService,
    ILogger<AnalyticsController> logger) : ControllerBase
{
    [HttpGet("daily-tasks/completion")]
    public async Task<IActionResult> GetTaskCompletion([FromQuery] DateTime start, [FromQuery] DateTime end)
    {
        logger.LogInformation("Fetching daily task completion analytics from {Start} to {End}", start, end);
        var data = await analyticsService.GetDailyTaskCompletionAsync(start, end);
        return Ok(data);
    }

    [HttpGet("breakdown/downtime")]
    public async Task<IActionResult> GetMachineDowntime([FromQuery] DateTime start, [FromQuery] DateTime end)
    {
        logger.LogInformation("Fetching machine downtime analytics from {Start} to {End}", start, end);
        var data = await analyticsService.GetMachineDowntimeAsync(start, end);
        return Ok(data);
    }

    [HttpGet("shift-handover")]
    public async Task<IActionResult> GetShiftHandover([FromQuery] DateTime start, [FromQuery] DateTime end)
    {
        logger.LogInformation("Fetching shift handover analytics from {Start} to {End}", start, end);
        var data = await analyticsService.GetShiftHandoverAnalyticsAsync(start, end);
        return Ok(data);
    }

    [HttpGet("sample-storage")]
    public async Task<IActionResult> GetSampleStorage([FromQuery] DateTime start, [FromQuery] DateTime end)
    {
        logger.LogInformation("Fetching sample storage analytics from {Start} to {End}", start, end);
        var data = await analyticsService.GetSampleStorageAnalyticsAsync(start, end);
        return Ok(data);
    }

    [HttpGet("calibration")]
    public async Task<IActionResult> GetCalibration([FromQuery] DateTime start, [FromQuery] DateTime end)
    {
        logger.LogInformation("Fetching calibration analytics from {Start} to {End}", start, end);
        var data = await analyticsService.GetCalibrationAnalyticsAsync(start, end);
        return Ok(data);
    }

    [HttpGet("kit-validation")]
    public async Task<IActionResult> GetKitValidation([FromQuery] DateTime start, [FromQuery] DateTime end)
    {
        logger.LogInformation("Fetching kit validation analytics from {Start} to {End}", start, end);
        var data = await analyticsService.GetKitValidationAnalyticsAsync(start, end);
        return Ok(data);
    }

    [HttpGet("media-sterility")]
    public async Task<IActionResult> GetMediaSterility([FromQuery] DateTime start, [FromQuery] DateTime end)
    {
        logger.LogInformation("Fetching media sterility analytics from {Start} to {End}", start, end);
        var data = await analyticsService.GetMediaSterilityAnalyticsAsync(start, end);
        return Ok(data);
    }

    [HttpGet("repeat-samples")]
    public async Task<IActionResult> GetRepeatSamples([FromQuery] DateTime start, [FromQuery] DateTime end)
    {
        logger.LogInformation("Fetching repeat samples analytics from {Start} to {End}", start, end);
        var data = await analyticsService.GetRepeatSampleAnalyticsAsync(start, end);
        return Ok(data);
    }
}