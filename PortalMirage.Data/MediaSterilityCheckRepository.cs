using Dapper;
using PortalMirage.Core.Models;
using PortalMirage.Core.Dtos;
using PortalMirage.Data.Abstractions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace PortalMirage.Data;

public class MediaSterilityCheckRepository(IDbConnectionFactory connectionFactory) : IMediaSterilityCheckRepository
{
    public async Task<MediaSterilityCheck> CreateAsync(MediaSterilityCheck sterilityCheck)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        return await connection.QuerySingleAsync<MediaSterilityCheck>(
            "usp_MediaSterilityChecks_Create",
            new { MediaName = sterilityCheck.MediaName, MediaLotNumber = sterilityCheck.MediaLotNumber, MediaQuantity = sterilityCheck.MediaQuantity, Result37C = sterilityCheck.Result37C, Result25C = sterilityCheck.Result25C, OverallStatus = sterilityCheck.OverallStatus, Comments = sterilityCheck.Comments, PerformedByUserID = sterilityCheck.PerformedByUserID },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<MediaSterilityCheck>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        return await connection.QueryAsync<MediaSterilityCheck>(
            "usp_MediaSterilityChecks_GetByDateRange",
            new { StartDate = startDate.Date, EndDate = endDate.Date },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<bool> DeactivateAsync(int checkId, int userId, string reason)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        var rowsAffected = await connection.ExecuteAsync(
            "usp_MediaSterilityChecks_Deactivate",
            new { CheckId = checkId, UserId = userId, Reason = reason },
            commandType: CommandType.StoredProcedure);
        return rowsAffected > 0;
    }

    public async Task<IEnumerable<MediaSterilityReportDto>> GetReportDataAsync(DateTime startDate, DateTime endDate, string? mediaName, string? status)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        return await connection.QueryAsync<MediaSterilityReportDto>(
            "usp_MediaSterilityChecks_GetReportData",
            new { StartDate = startDate.Date, EndDate = endDate.Date, MediaName = mediaName, Status = status },
            commandType: CommandType.StoredProcedure);
    }
}
