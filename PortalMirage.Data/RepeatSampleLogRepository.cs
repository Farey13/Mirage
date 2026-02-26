using Dapper;
using PortalMirage.Core.Models;
using PortalMirage.Data.Abstractions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using PortalMirage.Core.Dtos;

namespace PortalMirage.Data;

public class RepeatSampleLogRepository(IDbConnectionFactory connectionFactory) : IRepeatSampleLogRepository
{
    public async Task<RepeatSampleLog> CreateAsync(RepeatSampleLog repeatSampleLog)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        return await connection.QuerySingleAsync<RepeatSampleLog>(
            "usp_RepeatSampleLog_Create",
            new { PatientIdCardNumber = repeatSampleLog.PatientIdCardNumber, PatientName = repeatSampleLog.PatientName, ReasonText = repeatSampleLog.ReasonText, InformedPerson = repeatSampleLog.InformedPerson, Department = repeatSampleLog.Department, LoggedByUserID = repeatSampleLog.LoggedByUserID },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<RepeatSampleLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        return await connection.QueryAsync<RepeatSampleLog>(
            "usp_RepeatSampleLog_GetByDateRange",
            new { StartDate = startDate.Date, EndDate = endDate.Date },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<RepeatSampleReportDto>> GetReportDataAsync(DateTime startDate, DateTime endDate, string? reason, string? department)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        return await connection.QueryAsync<RepeatSampleReportDto>(
            "usp_RepeatSampleLog_GetReportData",
            new { StartDate = startDate.Date, EndDate = endDate.Date, Reason = reason, Department = department },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<bool> DeactivateAsync(int repeatId, int userId, string reason)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        var rowsAffected = await connection.ExecuteAsync(
            "usp_RepeatSampleLog_Deactivate",
            new { RepeatId = repeatId, UserId = userId, Reason = reason },
            commandType: CommandType.StoredProcedure);
        return rowsAffected > 0;
    }
}
