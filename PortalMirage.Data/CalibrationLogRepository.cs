using Dapper;
using PortalMirage.Core.Models;
using PortalMirage.Data.Abstractions;

namespace PortalMirage.Data;

public class CalibrationLogRepository(IDbConnectionFactory connectionFactory) : ICalibrationLogRepository
{
    public async Task<CalibrationLog> CreateAsync(CalibrationLog calibrationLog)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        const string sql = """
                           INSERT INTO CalibrationLogs (TestName, QcResult, Reason, PerformedByUserID)
                           OUTPUT INSERTED.*
                           VALUES (@TestName, @QcResult, @Reason, @PerformedByUserID);
                           """;
        return await connection.QuerySingleAsync<CalibrationLog>(sql, calibrationLog);
    }

    public async Task<IEnumerable<CalibrationLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        const string sql = "SELECT * FROM CalibrationLogs WHERE CalibrationDateTime BETWEEN @StartDate AND @EndDate ORDER BY CalibrationDateTime DESC";
        return await connection.QueryAsync<CalibrationLog>(sql, new { StartDate = startDate, EndDate = endDate });
    }
}