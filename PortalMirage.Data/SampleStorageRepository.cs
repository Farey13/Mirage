using Dapper;
using PortalMirage.Core.Models;
using PortalMirage.Core.Dtos;
using PortalMirage.Data.Abstractions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace PortalMirage.Data;

public class SampleStorageRepository(IDbConnectionFactory connectionFactory) : ISampleStorageRepository
{
    public async Task<SampleStorage> CreateAsync(SampleStorage sampleStorage)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        return await connection.QuerySingleAsync<SampleStorage>(
            "usp_SampleStorage_Create",
            new { PatientSampleID = sampleStorage.PatientSampleID, TestName = sampleStorage.TestName, StoredByUserID = sampleStorage.StoredByUserID },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<int> GetPendingCountAsync()
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        return await connection.ExecuteScalarAsync<int>(
            "usp_SampleStorage_GetPendingCount",
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<SampleStorage>> GetPendingByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        return await connection.QueryAsync<SampleStorage>(
            "usp_SampleStorage_GetPendingByDateRange",
            new { StartDate = startDate.Date, EndDate = endDate.Date },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<SampleStorage>> GetCompletedByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        return await connection.QueryAsync<SampleStorage>(
            "usp_SampleStorage_GetCompletedByDateRange",
            new { StartDate = startDate.Date, EndDate = endDate.Date },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<SampleStorage?> GetByIdAsync(int storageId)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        return await connection.QuerySingleOrDefaultAsync<SampleStorage>(
            "usp_SampleStorage_GetById",
            new { StorageId = storageId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<bool> MarkAsDoneAsync(int storageId, int userId)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        var rowsAffected = await connection.ExecuteAsync(
            "usp_SampleStorage_MarkAsDone",
            new { StorageId = storageId, UserId = userId },
            commandType: CommandType.StoredProcedure);
        return rowsAffected > 0;
    }

    public async Task<bool> DeactivateAsync(int storageId, int userId, string reason)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        var rowsAffected = await connection.ExecuteAsync(
            "usp_SampleStorage_Deactivate",
            new { StorageId = storageId, UserId = userId, Reason = reason },
            commandType: CommandType.StoredProcedure);
        return rowsAffected > 0;
    }

    public async Task<IEnumerable<SampleStorageReportDto>> GetReportDataAsync(DateTime startDate, DateTime endDate, string? testName, string? status)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        return await connection.QueryAsync<SampleStorageReportDto>(
            "usp_SampleStorage_GetReportData",
            new { StartDate = startDate.Date, EndDate = endDate.Date, TestName = testName, Status = status },
            commandType: CommandType.StoredProcedure);
    }
}
