using Dapper;
using PortalMirage.Core.Models;
using PortalMirage.Core.Dtos;
using PortalMirage.Data.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PortalMirage.Data;

public class MediaSterilityCheckRepository(IDbConnectionFactory connectionFactory) : IMediaSterilityCheckRepository
{
    public async Task<MediaSterilityCheck> CreateAsync(MediaSterilityCheck sterilityCheck)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        const string sql = """
                           INSERT INTO MediaSterilityChecks (MediaName, MediaLotNumber, MediaQuantity, Result37C, Result25C, OverallStatus, Comments, PerformedByUserID)
                           OUTPUT INSERTED.*
                           VALUES (@MediaName, @MediaLotNumber, @MediaQuantity, @Result37C, @Result25C, @OverallStatus, @Comments, @PerformedByUserID);
                           """;
        return await connection.QuerySingleAsync<MediaSterilityCheck>(sql, sterilityCheck);
    }

    public async Task<IEnumerable<MediaSterilityCheck>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        var inclusiveEndDate = endDate.Date.AddDays(1);
        const string sql = "SELECT * FROM MediaSterilityChecks WHERE IsActive = 1 AND CheckDateTime >= @StartDate AND CheckDateTime < @InclusiveEndDate ORDER BY CheckDateTime DESC";
        return await connection.QueryAsync<MediaSterilityCheck>(sql, new { StartDate = startDate.Date, InclusiveEndDate = inclusiveEndDate });
    }

    public async Task<bool> DeactivateAsync(int checkId, int userId, string reason)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        const string sql = """
                           UPDATE MediaSterilityChecks SET IsActive = 0, DeactivationReason = @Reason, 
                           DeactivatedByUserID = @UserId, DeactivationDateTime = GETDATE()
                           WHERE SterilityCheckID = @CheckId AND IsActive = 1;
                           """;
        var rowsAffected = await connection.ExecuteAsync(sql, new { CheckId = checkId, UserId = userId, Reason = reason });
        return rowsAffected > 0;
    }

    public async Task<IEnumerable<MediaSterilityReportDto>> GetReportDataAsync(DateTime startDate, DateTime endDate, string? mediaName, string? status)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        var inclusiveEndDate = endDate.Date.AddDays(1);

        var sqlBuilder = new StringBuilder(@"
            SELECT 
                m.CheckDateTime,
                m.MediaName,
                m.MediaLotNumber,
                m.MediaQuantity,
                m.Result37C,
                m.Result25C,
                m.OverallStatus,
                m.Comments,
                u.FullName AS PerformedByUsername
            FROM MediaSterilityChecks m
            LEFT JOIN Users u ON m.PerformedByUserID = u.UserID
            WHERE m.IsActive = 1 
              AND m.CheckDateTime >= @StartDate 
              AND m.CheckDateTime < @InclusiveEndDate
        ");

        var parameters = new DynamicParameters();
        parameters.Add("StartDate", startDate.Date);
        parameters.Add("InclusiveEndDate", inclusiveEndDate);

        if (!string.IsNullOrEmpty(mediaName) && mediaName != "All")
        {
            sqlBuilder.Append(" AND m.MediaName = @MediaName");
            parameters.Add("MediaName", mediaName);
        }

        if (!string.IsNullOrEmpty(status) && status != "All")
        {
            sqlBuilder.Append(" AND m.OverallStatus = @Status");
            parameters.Add("Status", status);
        }

        sqlBuilder.Append(" ORDER BY m.CheckDateTime DESC;");

        return await connection.QueryAsync<MediaSterilityReportDto>(sqlBuilder.ToString(), parameters);
    }
}