namespace PatientInfo.Api.Sdk.Models;

/// <summary>
/// Represents the patient information returned by the API.
/// </summary>
public record PatientInfoDto
(
    string? NationalID,
    string? HospitalNumber,
    string? PatientName,
    string Gender = "U",
    DateTime? DateOfBirth = null
);


