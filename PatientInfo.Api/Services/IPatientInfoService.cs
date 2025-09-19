using PatientInfo.Api.Controllers;
using PatientInfo.Api.Dtos;

namespace PatientInfo.Api.Services;

public interface IPatientInfoService
{
    PatientInfoDto? GetByHospitalNumber(HospitalNumber hospitalNumber);
    PatientInfoDto? GetByNationalID(NationalId nationalId);
}
