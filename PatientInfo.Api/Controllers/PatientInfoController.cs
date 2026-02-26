using Microsoft.AspNetCore.Mvc;
using PatientInfo.Api.Dtos;
using PatientInfo.Api.Services;

namespace PatientInfo.Api.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class PatientInfoController : ControllerBase
{
    private ILogger<PatientInfoController> _logger;
    private readonly IPatientInfoService _patientInfoService;

    public PatientInfoController(ILogger<PatientInfoController> logger, IPatientInfoService patientInfoService)
    {
        _logger = logger;
        _patientInfoService = patientInfoService;
    }

    [HttpPost("getbyhospitalnumber")]
    public async Task<ActionResult<PatientInfoDto>> GetByHospitalNumber([FromBody] HospitalNumber hospitalNumber)
    {
        _logger.LogInformation("GetByHospitalNumber called with {HospitalNumber}", hospitalNumber.Value);
        try
        {
            var patientInfo = await _patientInfoService.GetByHospitalNumber(hospitalNumber);
            if (patientInfo == null)
            {
                return NotFound();
            }
            return Ok(patientInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting patient by hospital number {HospitalNumber}", hospitalNumber.Value);
            return Problem(
                detail: ex.Message,
                title: "Error retrieving patient information",
                statusCode: 500);
        }
    }

    [HttpPost("getbynationalId")]
    public async Task<ActionResult<PatientInfoDto>> GetByNationalID([FromBody] NationalId nationalId)
    {
        _logger.LogInformation("GetByNationalID called with {NationalId}", nationalId.Value);
        try
        {
            var patientInfo = await _patientInfoService.GetByNationalID(nationalId);
            if (patientInfo == null)
            {
                return NotFound();
            }
            return Ok(patientInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting patient by national ID {NationalID}", nationalId.Value);
            return Problem(
                detail: ex.Message,
                title: "Error retrieving patient information",
                statusCode: 500);
        }
    }

}
