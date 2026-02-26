using Dapper;
using PortalMirage.Core.Models;
using PortalMirage.Core.Dtos;
using PortalMirage.Data.Abstractions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace PortalMirage.Data;

public class KitValidationRepository(IDbConnectionFactory connectionFactory) : IKitValidationRepository
{
    public async Task<KitValidation> CreateAsync(KitValidation kitValidation)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        return await connection.QuerySingleAsync<KitValidation>(
            "usp_KitValidations_Create",
            new { KitName = kitValidation.KitName, KitLotNumber = kitValidation.KitLotNumber, KitExpiryDate = kitValidation.KitExpiryDate, ValidationStatus = kitValidation.ValidationStatus, Comments = kitValidation.Comments, ValidatedByUserID = kitValidation.ValidatedByUserID },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<KitValidation>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        return await connection.QueryAsync<KitValidation>(
            "usp_KitValidations_GetByDateRange",
            new { StartDate = startDate.Date, EndDate = endDate.Date },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<KitValidationReportDto>> GetReportDataAsync(DateTime startDate, DateTime endDate, string? kitName, string? status)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        return await connection.QueryAsync<KitValidationReportDto>(
            "usp_KitValidations_GetReportData",
            new { StartDate = startDate.Date, EndDate = endDate.Date, KitName = kitName, Status = status },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<bool> DeactivateAsync(int validationId, int userId, string reason)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        var rowsAffected = await connection.ExecuteAsync(
            "usp_KitValidations_Deactivate",
            new { ValidationId = validationId, UserId = userId, Reason = reason },
            commandType: CommandType.StoredProcedure);
        return rowsAffected > 0;
    }
}
