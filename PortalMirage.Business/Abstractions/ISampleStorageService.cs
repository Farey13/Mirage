using PortalMirage.Core.Models;

namespace PortalMirage.Business.Abstractions;

public interface ISampleStorageService
{
    Task<SampleStorage> CreateAsync(SampleStorage sampleStorage);
    Task<IEnumerable<SampleStorage>> GetPendingByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<bool> MarkAsDoneAsync(int storageId, int userId);
}