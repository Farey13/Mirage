using Dapper;
using PortalMirage.Core.Models;
using PortalMirage.Data.Abstractions;

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
        const string sql = "SELECT * FROM MediaSterilityChecks WHERE CheckDateTime >= @StartDate AND CheckDateTime < @InclusiveEndDate ORDER BY CheckDateTime DESC";
        return await connection.QueryAsync<MediaSterilityCheck>(sql, new { StartDate = startDate.Date, InclusiveEndDate = inclusiveEndDate });
    }
}