using PortalMirage.Core.Models;

namespace PortalMirage.Data.Abstractions;

public interface ISampleStorageRepository
{
    Task<SampleStorage> CreateAsync(SampleStorage sampleStorage);
    Task<IEnumerable<SampleStorage>> GetPendingByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<SampleStorage?> GetByIdAsync(int storageId);
    Task<bool> MarkAsDoneAsync(int storageId, int userId);
}