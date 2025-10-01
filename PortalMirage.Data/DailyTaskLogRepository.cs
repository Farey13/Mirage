using Dapper;
using PortalMirage.Core.Models;
using PortalMirage.Data.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        const string sql = "SELECT COUNT(*) FROM DailyTaskLogs WHERE LogDate = @Date AND Status = 'Pending'";
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
}