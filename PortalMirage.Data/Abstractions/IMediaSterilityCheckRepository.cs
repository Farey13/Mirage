using PortalMirage.Core.Dtos;
using PortalMirage.Core.Models;

namespace PortalMirage.Data.Abstractions;

public interface IMediaSterilityCheckRepository
{
    Task<MediaSterilityCheck> CreateAsync(MediaSterilityCheck sterilityCheck);
    Task<IEnumerable<MediaSterilityCheck>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<bool> DeactivateAsync(int checkId, int userId, string reason);

    Task<IEnumerable<MediaSterilityReportDto>> GetReportDataAsync(DateTime startDate, DateTime endDate, string? mediaName, string? status);
}