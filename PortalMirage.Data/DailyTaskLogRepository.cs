using Dapper;
using PortalMirage.Core.Models;
using PortalMirage.Data.Abstractions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using PortalMirage.Core.Dtos;

namespace PortalMirage.Data;

public class DailyTaskLogRepository(IDbConnectionFactory connectionFactory) : IDailyTaskLogRepository
{
    public async Task<DailyTaskLog> CreateAsync(DailyTaskLog log)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        return await connection.QuerySingleAsync<DailyTaskLog>(
            "usp_DailyTaskLogs_Create",
            new { TaskID = log.TaskID, LogDate = log.LogDate, Status = log.Status, CompletedByUserID = log.CompletedByUserID, CompletedDateTime = log.CompletedDateTime, Comments = log.Comments },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<DailyTaskLog>> GetForDateAsync(DateTime date)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        return await connection.QueryAsync<DailyTaskLog>(
            "usp_DailyTaskLogs_GetForDate",
            new { Date = date.Date },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<DailyTaskLog?> UpdateStatusAsync(long logId, string status, int? userId, string? comment)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        return await connection.QuerySingleOrDefaultAsync<DailyTaskLog>(
            "usp_DailyTaskLogs_UpdateStatus",
            new { LogId = logId, Status = status, UserId = userId, Comment = comment },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<DailyTaskLog?> ExtendDeadlineAsync(long logId, DateTime newDeadline, string reason, int adminUserId)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        return await connection.QuerySingleOrDefaultAsync<DailyTaskLog>(
            "usp_DailyTaskLogs_ExtendDeadline",
            new { LogId = logId, NewDeadline = newDeadline, Reason = reason, AdminUserId = adminUserId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<int> GetPendingCountForDateAsync(DateTime date)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        return await connection.ExecuteScalarAsync<int>(
            "usp_DailyTaskLogs_GetPendingCount",
            new { Date = date.Date },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<DailyTaskLog?> OverrideLockAsync(long logId, DateTime overrideUntil, string reason, int adminUserId)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        return await connection.QuerySingleOrDefaultAsync<DailyTaskLog>(
            "usp_DailyTaskLogs_OverrideLock",
            new { LogId = logId, OverrideUntil = overrideUntil, Reason = reason, AdminUserId = adminUserId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<DailyTaskLogDto>> GetComplianceReportDataAsync(DateTime startDate, DateTime endDate, int? shiftId, string? status)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        return await connection.QueryAsync<DailyTaskLogDto>(
            "usp_DailyTaskLogs_GetComplianceReportData",
            new { StartDate = startDate.Date, EndDate = endDate.Date, ShiftId = shiftId, Status = status },
            commandType: CommandType.StoredProcedure);
    }
}
