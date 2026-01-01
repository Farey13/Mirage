using Dapper;
using PortalMirage.Core.Models;
using PortalMirage.Data.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PortalMirage.Core.Dtos;
using System.Text;

namespace PortalMirage.Data;

public class DailyTaskLogRepository(IDbConnectionFactory connectionFactory) : IDailyTaskLogRepository
{
    public async Task<DailyTaskLog> CreateAsync(DailyTaskLog log)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        const string sql = """
                           INSERT INTO DailyTaskLogs (TaskID, LogDate, Status, CompletedByUserID, CompletedDateTime, Comments)
                           OUTPUT INSERTED.*
                           VALUES (@TaskID, @LogDate, @Status, @CompletedByUserID, @CompletedDateTime, @Comments);
                           """;
        return await connection.QuerySingleAsync<DailyTaskLog>(sql, log);
    }

    public async Task<IEnumerable<DailyTaskLog>> GetForDateAsync(DateTime date)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        const string sql = "SELECT * FROM DailyTaskLogs WHERE LogDate = @Date";
        return await connection.QueryAsync<DailyTaskLog>(sql, new { Date = date.Date });
    }

    public async Task<DailyTaskLog?> UpdateStatusAsync(long logId, string status, int? userId, string? comment)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        var completedTime = status == "Completed" ? (DateTime?)DateTime.UtcNow : null;
        const string sql = """
                           UPDATE DailyTaskLogs
                           SET Status = @Status, 
                               CompletedByUserID = @UserId, 
                               CompletedDateTime = @CompletedTime,
                               Comments = @Comment
                           OUTPUT INSERTED.*
                           WHERE LogID = @LogId;
                           """;
        return await connection.QuerySingleOrDefaultAsync<DailyTaskLog>(sql, new
        {
            LogId = logId,
            Status = status,
            UserId = userId,
            Comment = comment,
            CompletedTime = completedTime
        });
    }

    public async Task<DailyTaskLog?> ExtendDeadlineAsync(long logId, DateTime newDeadline, string reason, int adminUserId)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        const string sql = """
                           UPDATE DailyTaskLogs
                           SET Status = 'Pending',
                               LockOverrideUntil = @NewDeadline,
                               LockOverrideReason = @Reason,
                               LockOverrideByUserID = @AdminUserId
                           OUTPUT INSERTED.*
                           WHERE LogID = @LogId;
                           """;
        return await connection.QuerySingleOrDefaultAsync<DailyTaskLog>(sql, new
        {
            LogId = logId,
            NewDeadline = newDeadline,
            Reason = reason,
            AdminUserId = adminUserId
        });
    }

    public async Task<int> GetPendingCountForDateAsync(DateTime date)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        const string sql = @"
            SELECT COUNT(dtl.LogID)
            FROM DailyTaskLogs dtl
            INNER JOIN Tasks t ON dtl.TaskID = t.TaskID
            INNER JOIN Shifts s ON t.ShiftID = s.ShiftID
            WHERE dtl.LogDate = @Date 
              AND (dtl.Status IS NULL OR 
                   dtl.Status NOT IN ('Complete', 'Completed'))  -- Fixed: Check for both 'Complete' and 'Completed'
              AND (
                dtl.LockOverrideUntil IS NULL OR dtl.LockOverrideUntil < GETDATE()
              )
              AND DATEADD(hour, s.GracePeriodHours, CAST(dtl.LogDate AS DATETIME) + CAST(s.EndTime AS DATETIME)) > GETDATE();
        ";
        return await connection.ExecuteScalarAsync<int>(sql, new { Date = date.Date });
    }

    public async Task<DailyTaskLog?> OverrideLockAsync(long logId, DateTime overrideUntil, string reason, int adminUserId)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        const string sql = """
                       UPDATE DailyTaskLogs
                       SET Status = 'Pending',
                           LockOverrideUntil = @OverrideUntil,
                           LockOverrideReason = @Reason,
                           LockOverrideByUserID = @AdminUserId
                       OUTPUT INSERTED.*
                       WHERE LogID = @LogId;
                       """;
        return await connection.QuerySingleOrDefaultAsync<DailyTaskLog>(sql, new
        {
            LogId = logId,
            OverrideUntil = overrideUntil,
            Reason = reason,
            AdminUserId = adminUserId
        });
    }

    public async Task<IEnumerable<DailyTaskLogDto>> GetComplianceReportDataAsync(DateTime startDate, DateTime endDate, int? shiftId, string? status)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        var inclusiveEndDate = endDate.Date.AddDays(1);

        var sqlBuilder = new StringBuilder(@"
            SELECT 
                dtl.LogDate,
                t.TaskName,
                s.ShiftName,
                dtl.Status,
                dtl.Comments,
                u.FullName AS CompletedByUserName,
                dtl.CompletedDateTime
            FROM DailyTaskLogs dtl
            INNER JOIN Tasks t ON dtl.TaskID = t.TaskID
            LEFT JOIN Shifts s ON t.ShiftID = s.ShiftID
            LEFT JOIN Users u ON dtl.CompletedByUserID = u.UserID
            WHERE dtl.LogDate >= @startDate AND dtl.LogDate < @inclusiveEndDate
        ");

        // Create anonymous object with parameter names that match SQL placeholders
        var parameters = new
        {
            startDate = startDate.Date,
            inclusiveEndDate,
            shiftId,
            status
        };

        if (shiftId.HasValue)
        {
            sqlBuilder.Append(" AND t.ShiftID = @shiftId");
        }

        if (!string.IsNullOrEmpty(status) && status != "All")
        {
            sqlBuilder.Append(" AND dtl.Status = @status");
        }

        sqlBuilder.Append(" ORDER BY dtl.LogDate, s.StartTime;");

        return await connection.QueryAsync<DailyTaskLogDto>(sqlBuilder.ToString(), parameters);
    }
}