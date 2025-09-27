using Dapper;
using PortalMirage.Core.Models;
using PortalMirage.Data.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortalMirage.Data;

public class RepeatSampleLogRepository(IDbConnectionFactory connectionFactory) : IRepeatSampleLogRepository
{
    public async Task<RepeatSampleLog> CreateAsync(RepeatSampleLog repeatSampleLog)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        const string sql = """
                           INSERT INTO RepeatSampleLog (PatientIdCardNumber, PatientName, ReasonText, InformedPerson, Department, LoggedByUserID)
                           OUTPUT INSERTED.*
                           VALUES (@PatientIdCardNumber, @PatientName, @ReasonText, @InformedPerson, @Department, @LoggedByUserID);
                           """;
        return await connection.QuerySingleAsync<RepeatSampleLog>(sql, repeatSampleLog);
    }

    public async Task<IEnumerable<RepeatSampleLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        var inclusiveEndDate = endDate.Date.AddDays(1);
        const string sql = "SELECT * FROM RepeatSampleLog WHERE IsActive = 1 AND LogDateTime >= @StartDate AND LogDateTime < @InclusiveEndDate ORDER BY LogDateTime DESC";
        return await connection.QueryAsync<RepeatSampleLog>(sql, new { StartDate = startDate.Date, InclusiveEndDate = inclusiveEndDate });
    }

    public async Task<bool> DeactivateAsync(int repeatId, int userId, string reason)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        const string sql = """
                           UPDATE RepeatSampleLog SET IsActive = 0, DeactivationReason = @Reason, 
                           DeactivatedByUserID = @UserId, DeactivationDateTime = GETDATE()
                           WHERE RepeatID = @RepeatId AND IsActive = 1;
                           """;
        var rowsAffected = await connection.ExecuteAsync(sql, new { RepeatId = repeatId, UserId = userId, Reason = reason });
        return rowsAffected > 0;
    }
}