using PortalMirage.Core.Models;

namespace PortalMirage.Data.Abstractions;

public interface IMediaSterilityCheckRepository
{
    Task<MediaSterilityCheck> CreateAsync(MediaSterilityCheck sterilityCheck);
    Task<IEnumerable<MediaSterilityCheck>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
}