namespace PatientInfo.Api.Dtos;

public class PatientInfoDto
{
    public string? NationalID { get; set; }
    public string? HospitalNumber { get; set; }
    public string? PatientName { get; set; }
    public string Gender { get; set; } = "U";
    public DateTime DateOfBirth { get; set; }
}
