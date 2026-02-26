using PatientInfo.Api.Controllers;
using PatientInfo.Api.Dtos;

namespace PatientInfo.Api.Services;

public interface IPatientInfoService
{
    Task<PatientInfoDto?> GetByHospitalNumber(HospitalNumber hospitalNumber);
    Task<PatientInfoDto?> GetByNationalID(NationalId nationalId);
}
