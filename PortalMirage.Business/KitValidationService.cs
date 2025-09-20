using PortalMirage.Business.Abstractions;
using PortalMirage.Core.Models;
using PortalMirage.Data.Abstractions;

namespace PortalMirage.Business;

public class KitValidationService(IKitValidationRepository kitValidationRepository) : IKitValidationService
{
    public async Task<KitValidation> CreateAsync(KitValidation kitValidation)
    {
        // In the future, any business rules (like checking for duplicate lot numbers) would go here.
        return await kitValidationRepository.CreateAsync(kitValidation);
    }

    public async Task<IEnumerable<KitValidation>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await kitValidationRepository.GetByDateRangeAsync(startDate, endDate);
    }
}