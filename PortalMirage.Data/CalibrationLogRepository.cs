using Dapper;
using PortalMirage.Core.Models;
using PortalMirage.Core.Dtos;
using PortalMirage.Data.Abstractions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace PortalMirage.Data;

public class CalibrationLogRepository(IDbConnectionFactory connectionFactory) : ICalibrationLogRepository
{
    public async Task<CalibrationLog> CreateAsync(CalibrationLog calibrationLog)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        return await connection.QuerySingleAsync<CalibrationLog>(
            "usp_CalibrationLogs_Create",
            new { TestName = calibrationLog.TestName, QcResult = calibrationLog.QcResult, Reason = calibrationLog.Reason, PerformedByUserID = calibrationLog.PerformedByUserID },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<bool> DeactivateAsync(int logId, int userId, string reason)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        var rowsAffected = await connection.ExecuteAsync(
            "usp_CalibrationLogs_Deactivate",
            new { LogId = logId, UserId = userId, Reason = reason },
            commandType: CommandType.StoredProcedure);
        return rowsAffected > 0;
    }

    public async Task<IEnumerable<CalibrationLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        return await connection.QueryAsync<CalibrationLog>(
            "usp_CalibrationLogs_GetByDateRange",
            new { StartDate = startDate.Date, EndDate = endDate.Date },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<CalibrationReportDto>> GetReportDataAsync(DateTime startDate, DateTime endDate, string? testName, string? qcResult)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        return await connection.QueryAsync<CalibrationReportDto>(
            "usp_CalibrationLogs_GetReportData",
            new { StartDate = startDate.Date, EndDate = endDate.Date, TestName = testName, QcResult = qcResult },
            commandType: CommandType.StoredProcedure);
    }
}
