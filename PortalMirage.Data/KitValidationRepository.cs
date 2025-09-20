using Dapper;
using PortalMirage.Core.Models;
using PortalMirage.Data.Abstractions;

namespace PortalMirage.Data;

public class KitValidationRepository(IDbConnectionFactory connectionFactory) : IKitValidationRepository
{
    public async Task<KitValidation> CreateAsync(KitValidation kitValidation)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        const string sql = """
                           INSERT INTO KitValidations (KitName, KitLotNumber, KitExpiryDate, ValidationStatus, Comments, ValidatedByUserID)
                           OUTPUT INSERTED.*
                           VALUES (@KitName, @KitLotNumber, @KitExpiryDate, @ValidationStatus, @Comments, @ValidatedByUserID);
                           """;
        return await connection.QuerySingleAsync<KitValidation>(sql, kitValidation);
    }

    public async Task<IEnumerable<KitValidation>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        const string sql = "SELECT * FROM KitValidations WHERE ValidationDateTime BETWEEN @StartDate AND @EndDate ORDER BY ValidationDateTime DESC";
        return await connection.QueryAsync<KitValidation>(sql, new { StartDate = startDate, EndDate = endDate });
    }
}