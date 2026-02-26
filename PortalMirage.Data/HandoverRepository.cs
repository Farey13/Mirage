using Dapper;
using PortalMirage.Core.Models;
using PortalMirage.Core.Dtos;
using PortalMirage.Data.Abstractions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace PortalMirage.Data;

public class HandoverRepository(IDbConnectionFactory connectionFactory) : IHandoverRepository
{
    public async Task<Handover> CreateAsync(Handover handover)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        return await connection.QuerySingleAsync<Handover>(
            "usp_Handovers_Create",
            new { HandoverNotes = handover.HandoverNotes, Priority = handover.Priority, Shift = handover.Shift, GivenByUserID = handover.GivenByUserID },
            commandType: CommandType.StoredProcedure);
    }
    
    public async Task<int> GetPendingCountAsync()
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        return await connection.ExecuteScalarAsync<int>(
            "usp_Handovers_GetPendingCount",
            commandType: CommandType.StoredProcedure);
    }
    
    public async Task<IEnumerable<Handover>> GetPendingAsync(DateTime startDate, DateTime endDate)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        return await connection.QueryAsync<Handover>(
            "usp_Handovers_GetPending",
            new { StartDate = startDate.Date, EndDate = endDate.Date },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<Handover>> GetCompletedAsync(DateTime startDate, DateTime endDate)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        return await connection.QueryAsync<Handover>(
            "usp_Handovers_GetCompleted",
            new { StartDate = startDate.Date, EndDate = endDate.Date },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<Handover?> GetByIdAsync(int handoverId)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        return await connection.QuerySingleOrDefaultAsync<Handover>(
            "usp_Handovers_GetById",
            new { HandoverId = handoverId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<HandoverReportDto>> GetReportDataAsync(DateTime startDate, DateTime endDate, string? shift, string? priority, string? status)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        return await connection.QueryAsync<HandoverReportDto>(
            "usp_Handovers_GetReportData",
            new { StartDate = startDate.Date, EndDate = endDate.Date, Shift = shift, Priority = priority, Status = status },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<bool> MarkAsReceivedAsync(int handoverId, int userId)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        var rowsAffected = await connection.ExecuteAsync(
            "usp_Handovers_MarkAsReceived",
            new { HandoverId = handoverId, UserId = userId },
            commandType: CommandType.StoredProcedure);
        return rowsAffected > 0;
    }

    public async Task<bool> DeactivateAsync(int handoverId, int userId, string reason)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        var rowsAffected = await connection.ExecuteAsync(
            "usp_Handovers_Deactivate",
            new { HandoverId = handoverId, UserId = userId, Reason = reason },
            commandType: CommandType.StoredProcedure);
        return rowsAffected > 0;
    }
}
