using Dapper;
using PortalMirage.Core.Models;
using PortalMirage.Data.Abstractions;

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
}