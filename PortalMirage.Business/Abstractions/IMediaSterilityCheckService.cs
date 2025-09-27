using PortalMirage.Core.Models;

namespace PortalMirage.Business.Abstractions;

public interface IMediaSterilityCheckService
{
    Task<MediaSterilityCheck> CreateAsync(MediaSterilityCheck sterilityCheck);
    Task<IEnumerable<MediaSterilityCheck>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

    Task<bool> DeactivateAsync(int checkId, int userId, string reason);
}