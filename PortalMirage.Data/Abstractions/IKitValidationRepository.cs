using PortalMirage.Core.Dtos;
using PortalMirage.Core.Models;

namespace PortalMirage.Data.Abstractions;

public interface IKitValidationRepository
{
    Task<KitValidation> CreateAsync(KitValidation kitValidation);
    Task<IEnumerable<KitValidation>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

    Task<bool> DeactivateAsync(int validationId, int userId, string reason);

    Task<IEnumerable<KitValidationReportDto>> GetReportDataAsync(DateTime startDate, DateTime endDate, string? kitName, string? status); // ADD THIS

}