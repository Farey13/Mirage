using PatientInfo.Api.Sdk.Models;
using Refit;

namespace PatientInfo.Api.Sdk;




/// <summary>
/// Refit interface for interacting with the PatientInfo API.
/// </summary>
public interface IPatientInfoApi
{
    /// <summary>
    /// Retrieves patient information using their hospital number.
    /// </summary>
    /// <param name="hospitalNumber">The hospital number payload.</param>
    /// <returns>A PatientInfoDto object.</returns>
    [Post("/api/PatientInfo/getbyhospitalnumber")]
    Task<PatientInfoDto> GetByHospitalNumberAsync([Body] HospitalNumber hospitalNumber);

    /// <summary>
    /// Retrieves patient information using their national ID.
    /// </summary>
    /// <param name="nationalId">The national ID payload.</param>
    /// <returns>A PatientInfoDto object.</returns>
    [Post("/api/PatientInfo/getbynationalId")]
    Task<PatientInfoDto> GetByNationalIdAsync([Body] NationalId nationalId);
}


