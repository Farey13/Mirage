using Dapper;
using PortalMirage.Core.Models;
using PortalMirage.Core.Dtos;
using PortalMirage.Data.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PortalMirage.Data;

public class HandoverRepository(IDbConnectionFactory connectionFactory) : IHandoverRepository
{
    public async Task<Handover> CreateAsync(Handover handover)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        const string sql = """
                           INSERT INTO Handovers (HandoverNotes, Priority, Shift, GivenByUserID)
                           OUTPUT INSERTED.*
                           VALUES (@HandoverNotes, @Priority, @Shift, @GivenByUserID);
                           """;
        return await connection.QuerySingleAsync<Handover>(sql, handover);
    }
    public async Task<int> GetPendingCountAsync()
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        // This SQL now correctly filters for today's date
        const string sql = @"
            SELECT COUNT(*) FROM Handovers 
            WHERE IsReceived = 0 AND IsActive = 1 
            AND GivenDateTime >= CAST(GETDATE() AS DATE) 
            AND GivenDateTime < DATEADD(day, 1, CAST(GETDATE() AS DATE))";
        return await connection.ExecuteScalarAsync<int>(sql);
    }
    public async Task<IEnumerable<Handover>> GetPendingAsync(DateTime startDate, DateTime endDate)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        var inclusiveEndDate = endDate.Date.AddDays(1);
        const string sql = "SELECT * FROM Handovers WHERE IsReceived = 0 AND IsActive = 1 AND GivenDateTime >= @StartDate AND GivenDateTime < @InclusiveEndDate ORDER BY GivenDateTime DESC";
        return await connection.QueryAsync<Handover>(sql, new { StartDate = startDate.Date, InclusiveEndDate = inclusiveEndDate });
    }

    public async Task<IEnumerable<Handover>> GetCompletedAsync(DateTime startDate, DateTime endDate)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        var inclusiveEndDate = endDate.Date.AddDays(1);
        const string sql = "SELECT * FROM Handovers WHERE IsReceived = 1 AND IsActive = 1 AND GivenDateTime >= @StartDate AND GivenDateTime < @InclusiveEndDate ORDER BY GivenDateTime DESC";
        return await connection.QueryAsync<Handover>(sql, new { StartDate = startDate.Date, InclusiveEndDate = inclusiveEndDate });
    }

    public async Task<Handover?> GetByIdAsync(int handoverId)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        const string sql = "SELECT * FROM Handovers WHERE HandoverID = @HandoverId";
        return await connection.QuerySingleOrDefaultAsync<Handover>(sql, new { HandoverId = handoverId });
    }

    public async Task<IEnumerable<HandoverReportDto>> GetReportDataAsync(DateTime startDate, DateTime endDate, string? shift, string? priority, string? status)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        var inclusiveEndDate = endDate.Date.AddDays(1);

        var sqlBuilder = new StringBuilder(@"
            SELECT 
                h.GivenDateTime,
                givenBy.FullName AS GivenByUsername,
                h.Shift,
                h.Priority,
                h.HandoverNotes,
                h.IsReceived,
                h.ReceivedDateTime,
                receivedBy.FullName AS ReceivedByUsername
            FROM Handovers h
            LEFT JOIN Users givenBy ON h.GivenByUserID = givenBy.UserID
            LEFT JOIN Users receivedBy ON h.ReceivedByUserID = receivedBy.UserID
            WHERE h.IsActive = 1
              AND h.GivenDateTime >= @StartDate 
              AND h.GivenDateTime < @InclusiveEndDate
        ");

        var parameters = new DynamicParameters();
        parameters.Add("StartDate", startDate.Date);
        parameters.Add("InclusiveEndDate", inclusiveEndDate);

        if (!string.IsNullOrEmpty(shift) && shift != "All")
        {
            sqlBuilder.Append(" AND h.Shift = @Shift");
            parameters.Add("Shift", shift);
        }

        if (!string.IsNullOrEmpty(priority) && priority != "All")
        {
            sqlBuilder.Append(" AND h.Priority = @Priority");
            parameters.Add("Priority", priority);
        }

        if (!string.IsNullOrEmpty(status))
        {
            if (status == "Pending") sqlBuilder.Append(" AND h.IsReceived = 0");
            else if (status == "Received") sqlBuilder.Append(" AND h.IsReceived = 1");
        }

        sqlBuilder.Append(" ORDER BY h.GivenDateTime DESC;");

        return await connection.QueryAsync<HandoverReportDto>(sqlBuilder.ToString(), parameters);
    }

    public async Task<bool> MarkAsReceivedAsync(int handoverId, int userId)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        const string sql = """
                           UPDATE Handovers 
                           SET IsReceived = 1, ReceivedByUserID = @UserId, ReceivedDateTime = GETDATE() 
                           WHERE HandoverID = @HandoverId AND IsReceived = 0;
                           """;
        var rowsAffected = await connection.ExecuteAsync(sql, new { HandoverId = handoverId, UserId = userId });
        return rowsAffected > 0;
    }

    public async Task<bool> DeactivateAsync(int handoverId, int userId, string reason)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        const string sql = """
                           UPDATE Handovers SET IsActive = 0, DeactivationReason = @Reason, 
                           DeactivatedByUserID = @UserId, DeactivationDateTime = GETDATE()
                           WHERE HandoverID = @HandoverId AND IsActive = 1;
                           """;
        var rowsAffected = await connection.ExecuteAsync(sql, new { HandoverId = handoverId, UserId = userId, Reason = reason });
        return rowsAffected > 0;
    }
}