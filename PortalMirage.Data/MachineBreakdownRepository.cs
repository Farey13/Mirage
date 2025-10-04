using Dapper;
using PortalMirage.Core.Dtos;
using PortalMirage.Core.Models;
using PortalMirage.Data.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

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

    public async Task<int> GetPendingCountAsync()
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        // This SQL now correctly counts ALL unresolved breakdowns from all time.
        const string sql = "SELECT COUNT(*) FROM MachineBreakdowns WHERE IsResolved = 0 AND IsActive = 1";
        return await connection.ExecuteScalarAsync<int>(sql);
    }

    public async Task<IEnumerable<MachineBreakdown>> GetPendingByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        var inclusiveEndDate = endDate.Date.AddDays(1);
        const string sql = """
                           SELECT * FROM MachineBreakdowns 
                           WHERE IsResolved = 0 AND IsActive = 1 
                           AND ReportedDateTime >= @StartDate AND ReportedDateTime < @InclusiveEndDate 
                           ORDER BY ReportedDateTime DESC
                           """;
        return await connection.QueryAsync<MachineBreakdown>(sql, new { StartDate = startDate.Date, InclusiveEndDate = inclusiveEndDate });
    }

    public async Task<MachineBreakdown?> GetByIdAsync(int breakdownId)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        const string sql = "SELECT * FROM MachineBreakdowns WHERE BreakdownID = @BreakdownId AND IsActive = 1";
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
                       WHERE IsResolved = 1 AND IsActive = 1 
                       AND ReportedDateTime >= @StartDate AND ReportedDateTime < @InclusiveEndDate 
                       ORDER BY ReportedDateTime DESC
                       """;
        return await connection.QueryAsync<MachineBreakdown>(sql, new { StartDate = startDate.Date, InclusiveEndDate = inclusiveEndDate });
    }

    public async Task<bool> DeactivateAsync(int breakdownId, int userId, string reason)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        const string sql = """
                       UPDATE MachineBreakdowns SET IsActive = 0, DeactivationReason = @Reason, 
                       DeactivatedByUserID = @UserId, DeactivationDateTime = GETDATE()
                       WHERE BreakdownID = @BreakdownId AND IsActive = 1;
                       """;
        var rowsAffected = await connection.ExecuteAsync(sql, new { BreakdownId = breakdownId, UserId = userId, Reason = reason });
        return rowsAffected > 0;
    }

    public async Task<IEnumerable<MachineBreakdownReportDto>> GetReportDataAsync(DateTime startDate, DateTime endDate, string? machineName, string? status)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        var inclusiveEndDate = endDate.Date.AddDays(1);

        var sqlBuilder = new StringBuilder(@"
            SELECT 
                mb.ReportedDateTime,
                mb.MachineName,
                mb.BreakdownReason,
                reporter.FullName AS ReportedByUsername,
                mb.IsResolved,
                mb.ResolvedDateTime,
                resolver.FullName AS ResolvedByUsername,
                mb.ResolutionNotes,
                mb.DowntimeMinutes
            FROM MachineBreakdowns mb
            LEFT JOIN Users reporter ON mb.ReportedByUserID = reporter.UserID
            LEFT JOIN Users resolver ON mb.ResolvedByUserID = resolver.UserID
            WHERE mb.IsActive = 1 
              AND mb.ReportedDateTime >= @StartDate 
              AND mb.ReportedDateTime < @InclusiveEndDate
        ");

        var parameters = new DynamicParameters();
        parameters.Add("StartDate", startDate.Date);
        parameters.Add("InclusiveEndDate", inclusiveEndDate);

        if (!string.IsNullOrEmpty(machineName))
        {
            sqlBuilder.Append(" AND mb.MachineName = @MachineName");
            parameters.Add("MachineName", machineName);
        }

        if (!string.IsNullOrEmpty(status))
        {
            if (status == "Pending")
            {
                sqlBuilder.Append(" AND mb.IsResolved = 0");
            }
            else if (status == "Resolved")
            {
                sqlBuilder.Append(" AND mb.IsResolved = 1");
            }
        }

        sqlBuilder.Append(" ORDER BY mb.ReportedDateTime DESC;");

        return await connection.QueryAsync<MachineBreakdownReportDto>(sqlBuilder.ToString(), parameters);
    }
}