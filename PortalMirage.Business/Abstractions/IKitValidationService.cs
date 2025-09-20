using PortalMirage.Core.Models;

namespace PortalMirage.Business.Abstractions;

public interface IKitValidationService
{
    Task<KitValidation> CreateAsync(KitValidation kitValidation);
    Task<IEnumerable<KitValidation>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
}