using PortalMirage.Business.Abstractions;
using PortalMirage.Core.Models;
using PortalMirage.Data.Abstractions;

namespace PortalMirage.Business;

public class MediaSterilityCheckService(IMediaSterilityCheckRepository sterilityCheckRepository) : IMediaSterilityCheckService
{
    public async Task<MediaSterilityCheck> CreateAsync(MediaSterilityCheck sterilityCheck)
    {
        // Business Rule: Automatically determine the overall status.
        if (sterilityCheck.Result25C.Equals("Growth Seen", StringComparison.OrdinalIgnoreCase) ||
            sterilityCheck.Result37C.Equals("Growth Seen", StringComparison.OrdinalIgnoreCase))
        {
            sterilityCheck.OverallStatus = "Failed";
        }
        else
        {
            sterilityCheck.OverallStatus = "Passed";
        }

        return await sterilityCheckRepository.CreateAsync(sterilityCheck);
    }

    public async Task<IEnumerable<MediaSterilityCheck>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await sterilityCheckRepository.GetByDateRangeAsync(startDate, endDate);
    }
}