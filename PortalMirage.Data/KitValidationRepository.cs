using Dapper;
using PortalMirage.Core.Models;
using PortalMirage.Core.Dtos;
using PortalMirage.Data.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

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

        const string sql = "SELECT * FROM KitValidations WHERE IsActive = 1 AND ValidationDateTime >= @StartDate AND ValidationDateTime < @InclusiveEndDate ORDER BY ValidationDateTime DESC";

        return await connection.QueryAsync<KitValidation>(sql, new { StartDate = startDate.Date, InclusiveEndDate = inclusiveEndDate });
    }

    public async Task<IEnumerable<KitValidationReportDto>> GetReportDataAsync(DateTime startDate, DateTime endDate, string? kitName, string? status)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        var inclusiveEndDate = endDate.Date.AddDays(1);

        var sqlBuilder = new StringBuilder(@"
            SELECT 
                kv.ValidationDateTime,
                kv.KitName,
                kv.KitLotNumber,
                kv.KitExpiryDate,
                kv.ValidationStatus,
                kv.Comments,
                u.FullName AS ValidatedByUsername
            FROM KitValidations kv
            LEFT JOIN Users u ON kv.ValidatedByUserID = u.UserID
            WHERE kv.IsActive = 1 
              AND kv.ValidationDateTime >= @StartDate 
              AND kv.ValidationDateTime < @InclusiveEndDate
        ");

        var parameters = new DynamicParameters();
        parameters.Add("StartDate", startDate.Date);
        parameters.Add("InclusiveEndDate", inclusiveEndDate);

        if (!string.IsNullOrEmpty(kitName) && kitName != "All")
        {
            sqlBuilder.Append(" AND kv.KitName = @KitName");
            parameters.Add("KitName", kitName);
        }

        if (!string.IsNullOrEmpty(status) && status != "All")
        {
            sqlBuilder.Append(" AND kv.ValidationStatus = @Status");
            parameters.Add("Status", status);
        }

        sqlBuilder.Append(" ORDER BY kv.ValidationDateTime DESC;");

        return await connection.QueryAsync<KitValidationReportDto>(sqlBuilder.ToString(), parameters);
    }

    public async Task<bool> DeactivateAsync(int validationId, int userId, string reason)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        const string sql = """
                           UPDATE KitValidations SET IsActive = 0, DeactivationReason = @Reason, 
                           DeactivatedByUserID = @UserId, DeactivationDateTime = GETDATE()
                           WHERE ValidationID = @ValidationId AND IsActive = 1;
                           """;
        var rowsAffected = await connection.ExecuteAsync(sql, new { ValidationId = validationId, UserId = userId, Reason = reason });
        return rowsAffected > 0;
    }
}