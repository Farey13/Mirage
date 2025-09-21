using Dapper;
using PortalMirage.Core.Models;
using PortalMirage.Data.Abstractions;

namespace PortalMirage.Data;

public class DailyTaskLogRepository(IDbConnectionFactory connectionFactory) : IDailyTaskLogRepository
{
    public async Task<DailyTaskLog> CreateAsync(DailyTaskLog log)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        const string sql = """
                           INSERT INTO DailyTaskLogs (TaskID, LogDate, Status, CompletedByUserID, CompletedDateTime)
                           OUTPUT INSERTED.*
                           VALUES (@TaskID, @LogDate, @Status, @CompletedByUserID, @CompletedDateTime);
                           """;
        return await connection.QuerySingleAsync<DailyTaskLog>(sql, log);
    }

    public async Task<IEnumerable<DailyTaskLog>> GetForDateAsync(DateTime date) // Changed from DateOnly
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        // The SQL DATE column can be correctly compared with a C# DateTime parameter
        const string sql = "SELECT * FROM DailyTaskLogs WHERE LogDate = @Date";
        return await connection.QueryAsync<DailyTaskLog>(sql, new { Date = date.Date });
    }

    public async Task<DailyTaskLog?> UpdateStatusAsync(long logId, string status, int? userId)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        const string sql = """
                           UPDATE DailyTaskLogs
                           SET Status = @Status, CompletedByUserID = @UserId, CompletedDateTime = GETDATE()
                           OUTPUT INSERTED.*
                           WHERE LogID = @LogId;
                           """;
        return await connection.QuerySingleOrDefaultAsync<DailyTaskLog>(sql, new { LogId = logId, Status = status, UserId = userId });
    }
}