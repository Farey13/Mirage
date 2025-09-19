using PatientInfo.Api.Controllers;
using PatientInfo.Api.Dtos;

namespace PatientInfo.Api.Services;

public class PatientInfoService : IPatientInfoService
{
    private List<PatientInfoDto> _demoData;

    public PatientInfoService()
    {
        _demoData = [
            new PatientInfoDto { NationalID = "A111111", HospitalNumber = "1111", PatientName = "John Doe" },
            new PatientInfoDto { NationalID = "A222222", HospitalNumber = "2222", PatientName = "Jane Smith" }
        ];
    }
    public PatientInfoDto? GetByHospitalNumber(HospitalNumber hospitalNumber)
    {
        return _demoData.FirstOrDefault(p => p.HospitalNumber == hospitalNumber.Value.ToString());
    }

    public PatientInfoDto? GetByNationalID(NationalId nationalId)
    {
        return _demoData.FirstOrDefault(p => p.NationalID == nationalId.Value);
    }
}