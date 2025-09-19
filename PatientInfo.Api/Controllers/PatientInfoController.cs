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
    public ActionResult<PatientInfoDto> GetByHospitalNumber([FromBody] HospitalNumber hospitalNumber)
    {
        _logger.LogInformation("GetByHospitalNumber called with {HospitalNumber}", hospitalNumber.Value);
        var patientInfo = _patientInfoService.GetByHospitalNumber(hospitalNumber);
        if (patientInfo == null)
        {
            return NotFound();
        }
        return Ok(patientInfo);
    }

    [HttpPost("getbynationalId")]
    public ActionResult<PatientInfoDto> GetByNationalID([FromBody] NationalId hospitalNumber)
    {
        _logger.LogInformation("GetByNationalID called with {NationalId}", hospitalNumber.Value);
        var patientInfo = _patientInfoService.GetByNationalID(hospitalNumber);
        if (patientInfo == null)
        {
            return NotFound();
        }
        return Ok(patientInfo);
    }

}
