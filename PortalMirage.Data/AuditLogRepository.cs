using Dapper;
using PortalMirage.Core.Dtos;
using PortalMirage.Core.Models;
using PortalMirage.Data.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortalMirage.Data;

public class AuditLogRepository(IDbConnectionFactory connectionFactory) : IAuditLogRepository
{
    public async Task<IEnumerable<AuditLogDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        var inclusiveEndDate = endDate.Date.AddDays(1);

        const string sql = """
                           SELECT 
                               a.AuditID, a.UserID, u.FullName AS UserFullName, a.Timestamp,
                               a.ActionType, a.ModuleName, a.RecordID, a.FieldName,
                               a.OldValue, a.NewValue
                           FROM AuditLog a
                           LEFT JOIN Users u ON a.UserID = u.UserID
                           WHERE a.Timestamp >= @StartDate AND a.Timestamp < @InclusiveEndDate
                           ORDER BY a.Timestamp DESC;
                           """;
        return await connection.QueryAsync<AuditLogDto>(sql, new { StartDate = startDate.Date, InclusiveEndDate = inclusiveEndDate });
    }

    public async System.Threading.Tasks. Task CreateAsync(AuditLog logEntry)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        const string sql = """
                           INSERT INTO AuditLog (UserID, ActionType, ModuleName, RecordID, FieldName, OldValue, NewValue)
                           VALUES (@UserID, @ActionType, @ModuleName, @RecordID, @FieldName, @OldValue, @NewValue);
                           """;
        await connection.ExecuteAsync(sql, logEntry);
    }
}