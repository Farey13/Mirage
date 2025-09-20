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

    // This is the single, correct version of the method
    public async Task<IEnumerable<KitValidation>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();

        // This adjusts the end date to be the start of the *next* day, 
        // ensuring we capture everything within the selected end date.
        var inclusiveEndDate = endDate.Date.AddDays(1);

        const string sql = "SELECT * FROM KitValidations WHERE ValidationDateTime >= @StartDate AND ValidationDateTime < @InclusiveEndDate ORDER BY ValidationDateTime DESC";

        return await connection.QueryAsync<KitValidation>(sql, new { StartDate = startDate.Date, InclusiveEndDate = inclusiveEndDate });
    }
}