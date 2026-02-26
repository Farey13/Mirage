using Dapper;
using PortalMirage.Core.Dtos;
using PortalMirage.Core.Models;
using PortalMirage.Data.Abstractions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace PortalMirage.Data;

public class AuditLogRepository(IDbConnectionFactory connectionFactory) : IAuditLogRepository
{
    public async Task<IEnumerable<AuditLogDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        return await connection.QueryAsync<AuditLogDto>(
            "usp_AuditLog_GetByDateRange",
            new { StartDate = startDate.Date, EndDate = endDate.Date },
            commandType: CommandType.StoredProcedure);
    }

    public async System.Threading.Tasks.Task CreateAsync(AuditLog logEntry)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        await connection.ExecuteAsync(
            "usp_AuditLog_Create",
            new { UserID = logEntry.UserID, ActionType = logEntry.ActionType, ModuleName = logEntry.ModuleName, RecordID = logEntry.RecordID, FieldName = logEntry.FieldName, OldValue = logEntry.OldValue, NewValue = logEntry.NewValue },
            commandType: CommandType.StoredProcedure);
    }
}
