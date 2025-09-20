using PortalMirage.Core.Models;

namespace PortalMirage.Data.Abstractions;

public interface IKitValidationRepository
{
    Task<KitValidation> CreateAsync(KitValidation kitValidation);
    Task<IEnumerable<KitValidation>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
}