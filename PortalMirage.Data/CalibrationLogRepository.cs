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

        // This adjusts the end date to be the start of the *next* day, 
        // ensuring we capture everything within the selected end date.
        var inclusiveEndDate = endDate.Date.AddDays(1);

        const string sql = "SELECT * FROM CalibrationLogs WHERE CalibrationDateTime >= @StartDate AND CalibrationDateTime < @InclusiveEndDate ORDER BY CalibrationDateTime DESC";

        return await connection.QueryAsync<CalibrationLog>(sql, new { StartDate = startDate.Date, InclusiveEndDate = inclusiveEndDate });
    }
}