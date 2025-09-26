using Dapper;
using PortalMirage.Core.Models;
using PortalMirage.Data.Abstractions;

namespace PortalMirage.Data;

public class MachineBreakdownRepository(IDbConnectionFactory connectionFactory) : IMachineBreakdownRepository
{
    public async Task<MachineBreakdown> CreateAsync(MachineBreakdown breakdown)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        const string sql = """
                           INSERT INTO MachineBreakdowns (MachineName, BreakdownReason, ReportedByUserID)
                           OUTPUT INSERTED.*
                           VALUES (@MachineName, @BreakdownReason, @ReportedByUserID);
                           """;
        return await connection.QuerySingleAsync<MachineBreakdown>(sql, breakdown);
    }

    public async Task<IEnumerable<MachineBreakdown>> GetPendingByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        var inclusiveEndDate = endDate.Date.AddDays(1);
        const string sql = """
                           SELECT * FROM MachineBreakdowns 
                           WHERE IsResolved = 0 
                           AND ReportedDateTime >= @StartDate AND ReportedDateTime < @InclusiveEndDate 
                           ORDER BY ReportedDateTime DESC
                           """;
        return await connection.QueryAsync<MachineBreakdown>(sql, new { StartDate = startDate.Date, InclusiveEndDate = inclusiveEndDate });
    }

    public async Task<MachineBreakdown?> GetByIdAsync(int breakdownId)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        const string sql = "SELECT * FROM MachineBreakdowns WHERE BreakdownID = @BreakdownId";
        return await connection.QuerySingleOrDefaultAsync<MachineBreakdown>(sql, new { BreakdownId = breakdownId });
    }

    public async Task<bool> MarkAsResolvedAsync(int breakdownId, int userId, string resolutionNotes)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        const string sql = """
                       UPDATE MachineBreakdowns 
                       SET IsResolved = 1, 
                           ResolvedByUserID = @UserId, 
                           ResolvedDateTime = GETDATE(),
                           ResolutionNotes = @ResolutionNotes,
                           DowntimeMinutes = DATEDIFF(minute, ReportedDateTime, GETDATE())
                       WHERE BreakdownID = @BreakdownId AND IsResolved = 0;
                       """;
        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            BreakdownId = breakdownId,
            UserId = userId,
            ResolutionNotes = resolutionNotes
        });
        return rowsAffected > 0;
    }

    public async Task<IEnumerable<MachineBreakdown>> GetResolvedByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        var inclusiveEndDate = endDate.Date.AddDays(1);
        const string sql = """
                       SELECT * FROM MachineBreakdowns 
                       WHERE IsResolved = 1 
                       AND ReportedDateTime >= @StartDate AND ReportedDateTime < @InclusiveEndDate 
                       ORDER BY ReportedDateTime DESC
                       """;
        return await connection.QueryAsync<MachineBreakdown>(sql, new { StartDate = startDate.Date, InclusiveEndDate = inclusiveEndDate });
    }
}