using Dapper;
using PatientInfo.Api.Dtos;
using System.Data;

namespace PatientInfo.Api.Services;

public class PatientInfoService : IPatientInfoService
{
    private readonly IDbConnection _dbConnection;
    private readonly ILogger<PatientInfoService> _logger;

    public PatientInfoService(IDbConnection dbConnection, ILogger<PatientInfoService> logger)
    {
        _dbConnection = dbConnection;
        _logger = logger;
    }

    public async Task<PatientInfoDto?> GetByHospitalNumber(HospitalNumber hospitalNumber)
    {
        try
        {
            const string sql = @"
                SELECT 
                    LTRIM(RTRIM([IDCard])) AS [NationalID],
                    [ptidxNo] AS [HospitalNumber],
                    LTRIM(RTRIM([ptName])) AS [PatientName],
                    [sex] AS [Gender],
                    [DOB] AS [DateOfBirth]
                FROM dbo.ptInfo p
                WHERE p.ptidxNo = @PatientID";

            var patient = await _dbConnection.QueryFirstOrDefaultAsync<PatientInfoDto>(
                sql, new { PatientID = hospitalNumber.Value });

            if (patient is null) { return null; }

            if (string.IsNullOrWhiteSpace(patient.Gender)) { patient.Gender = "U"; }
            patient.PatientName = patient.PatientName?.Trim();
            patient.Gender = patient.Gender.ToUpper();

            return patient;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error querying patient by hospital number {HospitalNumber}", hospitalNumber.Value);
            throw;
        }
    }

    public async Task<PatientInfoDto?> GetByNationalID(NationalId nationalId)
    {
        try
        {
            const string sql = @"
                SELECT 
                    LTRIM(RTRIM([IDCard])) AS [NationalID],
                    [ptidxNo] AS [HospitalNumber],
                    LTRIM(RTRIM([ptName])) AS [PatientName],
                    [sex] AS [Gender],
                    [DOB] AS [DateOfBirth]
                FROM dbo.ptInfo p
                WHERE p.IDCard = @NationalID";

            var patient = await _dbConnection.QueryFirstOrDefaultAsync<PatientInfoDto>(
                sql, new { NationalID = nationalId.Value });

            if (patient is null) { return null; }

            if (string.IsNullOrWhiteSpace(patient.Gender)) { patient.Gender = "U"; }
            patient.PatientName =  patient.PatientName?.Trim();
            patient.Gender = patient.Gender.ToUpper();

            return patient;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error querying patient by national ID {NationalID}", nationalId.Value);
            throw;
        }
    }
}