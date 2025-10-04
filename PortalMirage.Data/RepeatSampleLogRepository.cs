using Dapper;
using PortalMirage.Core.Models;
using PortalMirage.Data.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PortalMirage.Core.Dtos;
using System.Text;

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

    public async Task<IEnumerable<RepeatSampleReportDto>> GetReportDataAsync(DateTime startDate, DateTime endDate, string? reason, string? department)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        var inclusiveEndDate = endDate.Date.AddDays(1);

        var sqlBuilder = new StringBuilder(@"
            SELECT 
                r.LogDateTime,
                r.PatientIdCardNumber,
                r.PatientName,
                r.ReasonText,
                r.Department,
                r.InformedPerson,
                u.FullName AS LoggedByUsername
            FROM RepeatSampleLog r
            LEFT JOIN Users u ON r.LoggedByUserID = u.UserID
            WHERE r.IsActive = 1 
              AND r.LogDateTime >= @StartDate 
              AND r.LogDateTime < @InclusiveEndDate
        ");

        var parameters = new DynamicParameters();
        parameters.Add("StartDate", startDate.Date);
        parameters.Add("InclusiveEndDate", inclusiveEndDate);

        if (!string.IsNullOrEmpty(reason) && reason != "All")
        {
            sqlBuilder.Append(" AND r.ReasonText = @Reason");
            parameters.Add("Reason", reason);
        }

        if (!string.IsNullOrEmpty(department) && department != "All")
        {
            sqlBuilder.Append(" AND r.Department = @Department");
            parameters.Add("Department", department);
        }

        sqlBuilder.Append(" ORDER BY r.LogDateTime DESC;");

        return await connection.QueryAsync<RepeatSampleReportDto>(sqlBuilder.ToString(), parameters);
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