using PortalMirage.Core.Models;

namespace PortalMirage.Business.Abstractions;

public interface IKitValidationService
{
    Task<KitValidation> CreateAsync(KitValidation kitValidation);
    Task<IEnumerable<KitValidation>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

    Task<bool> DeactivateAsync(int validationId, int userId, string reason);

}