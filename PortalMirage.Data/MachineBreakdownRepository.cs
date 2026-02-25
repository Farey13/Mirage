using Dapper;
using PortalMirage.Core.Dtos;
using PortalMirage.Core.Models;
using PortalMirage.Data.Abstractions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace PortalMirage.Data;

public class MachineBreakdownRepository(IDbConnectionFactory connectionFactory) : IMachineBreakdownRepository
{
    public async Task<MachineBreakdown> CreateAsync(MachineBreakdown breakdown)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        return await connection.QuerySingleAsync<MachineBreakdown>(
            "usp_MachineBreakdowns_Create",
            new { MachineName = breakdown.MachineName, BreakdownReason = breakdown.BreakdownReason, ReportedByUserID = breakdown.ReportedByUserID },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<int> GetPendingCountAsync()
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        return await connection.ExecuteScalarAsync<int>(
            "usp_MachineBreakdowns_GetPendingCount",
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<MachineBreakdown>> GetPendingByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        return await connection.QueryAsync<MachineBreakdown>(
            "usp_MachineBreakdowns_GetPendingByDateRange",
            new { StartDate = startDate.Date, EndDate = endDate.Date },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<MachineBreakdown?> GetByIdAsync(int breakdownId)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        return await connection.QuerySingleOrDefaultAsync<MachineBreakdown>(
            "usp_MachineBreakdowns_GetById",
            new { BreakdownId = breakdownId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<bool> MarkAsResolvedAsync(int breakdownId, int userId, string resolutionNotes)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        var rowsAffected = await connection.ExecuteAsync(
            "usp_MachineBreakdowns_MarkAsResolved",
            new { BreakdownId = breakdownId, UserId = userId, ResolutionNotes = resolutionNotes },
            commandType: CommandType.StoredProcedure);
        return rowsAffected > 0;
    }

    public async Task<IEnumerable<MachineBreakdown>> GetResolvedByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        return await connection.QueryAsync<MachineBreakdown>(
            "usp_MachineBreakdowns_GetResolvedByDateRange",
            new { StartDate = startDate.Date, EndDate = endDate.Date },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<bool> DeactivateAsync(int breakdownId, int userId, string reason)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        var rowsAffected = await connection.ExecuteAsync(
            "usp_MachineBreakdowns_Deactivate",
            new { BreakdownId = breakdownId, UserId = userId, Reason = reason },
            commandType: CommandType.StoredProcedure);
        return rowsAffected > 0;
    }

    public async Task<IEnumerable<MachineBreakdownReportDto>> GetReportDataAsync(DateTime startDate, DateTime endDate, string? machineName, string? status)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        return await connection.QueryAsync<MachineBreakdownReportDto>(
            "usp_MachineBreakdowns_GetReportData",
            new { StartDate = startDate.Date, EndDate = endDate.Date, MachineName = machineName, Status = status },
            commandType: CommandType.StoredProcedure);
    }
}
