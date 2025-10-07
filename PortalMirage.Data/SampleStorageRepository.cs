using Dapper;
using PortalMirage.Core.Models;
using PortalMirage.Core.Dtos;
using PortalMirage.Data.Abstractions;
using System.Text;

namespace PortalMirage.Data;

public class SampleStorageRepository(IDbConnectionFactory connectionFactory) : ISampleStorageRepository
{
    // Replace the CreateAsync method with this new version
    public async Task<SampleStorage> CreateAsync(SampleStorage sampleStorage)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        const string sql = """
                       INSERT INTO SampleStorage (PatientSampleID, TestName, StoredByUserID)
                       OUTPUT INSERTED.*
                       VALUES (@PatientSampleID, @TestName, @StoredByUserID);
                       """;
        return await connection.QuerySingleAsync<SampleStorage>(sql, sampleStorage);
    }

    public async Task<int> GetPendingCountAsync()
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        const string sql = @"
            SELECT COUNT(*) FROM SampleStorage 
            WHERE IsTestDone = 0 
              AND IsActive = 1 
              AND StorageDateTime >= CAST(GETDATE() AS DATE) 
              AND StorageDateTime < DATEADD(day, 1, CAST(GETDATE() AS DATE))";
        return await connection.ExecuteScalarAsync<int>(sql);
    }

    public async Task<IEnumerable<SampleStorage>> GetPendingByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        var inclusiveEndDate = endDate.Date.AddDays(1);
        const string sql = """
                           SELECT * FROM SampleStorage 
                           WHERE IsTestDone = 0 AND IsActive = 1 
                           AND StorageDateTime >= @StartDate AND StorageDateTime < @InclusiveEndDate 
                           ORDER BY StorageDateTime DESC
                           """;
        return await connection.QueryAsync<SampleStorage>(sql, new { StartDate = startDate.Date, InclusiveEndDate = inclusiveEndDate });
    }

    public async Task<IEnumerable<SampleStorage>> GetCompletedByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        var inclusiveEndDate = endDate.Date.AddDays(1);
        const string sql = """
                       SELECT * FROM SampleStorage 
                       WHERE IsTestDone = 1 AND IsActive = 1
                       AND StorageDateTime >= @StartDate AND StorageDateTime < @InclusiveEndDate 
                       ORDER BY StorageDateTime DESC
                       """;
        return await connection.QueryAsync<SampleStorage>(sql, new { StartDate = startDate.Date, InclusiveEndDate = inclusiveEndDate });
    }

    public async Task<SampleStorage?> GetByIdAsync(int storageId)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        const string sql = "SELECT * FROM SampleStorage WHERE StorageID = @StorageId";
        return await connection.QuerySingleOrDefaultAsync<SampleStorage>(sql, new { StorageId = storageId });
    }

    public async Task<bool> MarkAsDoneAsync(int storageId, int userId)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        const string sql = """
                           UPDATE SampleStorage 
                           SET IsTestDone = 1, TestDoneByUserID = @UserId, TestDoneDateTime = GETDATE() 
                           WHERE StorageID = @StorageId AND IsTestDone = 0;
                           """;
        var rowsAffected = await connection.ExecuteAsync(sql, new { StorageId = storageId, UserId = userId });
        return rowsAffected > 0;
    }

    // Replace the DeactivateAsync method
    public async Task<bool> DeactivateAsync(int storageId, int userId, string reason)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        const string sql = """
                       UPDATE SampleStorage 
                       SET IsActive = 0, 
                           DeactivationReason = @Reason,
                           DeactivatedByUserID = @UserId,
                           DeactivationDateTime = GETDATE()
                       WHERE StorageID = @StorageId AND IsActive = 1;
                       """;
        var rowsAffected = await connection.ExecuteAsync(sql, new { StorageId = storageId, UserId = userId, Reason = reason });
        return rowsAffected > 0;
    }

    public async Task<IEnumerable<SampleStorageReportDto>> GetReportDataAsync(DateTime startDate, DateTime endDate, string? testName, string? status)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        var inclusiveEndDate = endDate.Date.AddDays(1);

        var sqlBuilder = new StringBuilder(@"
            SELECT 
                ss.StorageDateTime,
                ss.PatientSampleID,
                ss.TestName,
                u_stored.FullName AS StoredByUsername,
                ss.IsTestDone,
                ss.TestDoneDateTime,
                u_done.FullName AS TestDoneByUsername
            FROM SampleStorage ss
            LEFT JOIN Users u_stored ON ss.StoredByUserID = u_stored.UserID
            LEFT JOIN Users u_done ON ss.TestDoneByUserID = u_done.UserID
            WHERE ss.IsActive = 1 
              AND ss.StorageDateTime >= @StartDate 
              AND ss.StorageDateTime < @InclusiveEndDate
        ");

        var parameters = new DynamicParameters();
        parameters.Add("StartDate", startDate.Date);
        parameters.Add("InclusiveEndDate", inclusiveEndDate);

        if (!string.IsNullOrEmpty(testName) && testName != "All")
        {
            sqlBuilder.Append(" AND ss.TestName = @TestName");
            parameters.Add("TestName", testName);
        }

        if (!string.IsNullOrEmpty(status))
        {
            if (status == "Pending") sqlBuilder.Append(" AND ss.IsTestDone = 0");
            else if (status == "Test Done") sqlBuilder.Append(" AND ss.IsTestDone = 1");
        }

        sqlBuilder.Append(" ORDER BY ss.StorageDateTime DESC;");

        return await connection.QueryAsync<SampleStorageReportDto>(sqlBuilder.ToString(), parameters);
    }
}