using PortalMirage.Business.Abstractions;
using PortalMirage.Core.Models;
using PortalMirage.Data.Abstractions;

namespace PortalMirage.Business;

public class SampleStorageService(ISampleStorageRepository sampleStorageRepository) : ISampleStorageService
{
    public async Task<SampleStorage> CreateAsync(SampleStorage sampleStorage)
    {
        return await sampleStorageRepository.CreateAsync(sampleStorage);
    }

    public async Task<IEnumerable<SampleStorage>> GetPendingByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await sampleStorageRepository.GetPendingByDateRangeAsync(startDate, endDate);
    }

    public async Task<bool> MarkAsDoneAsync(int storageId, int userId)
    {
        // 1. Business Rule: First, check if the sample exists.
        var sample = await sampleStorageRepository.GetByIdAsync(storageId);
        if (sample is null)
        {
            return false; // Can't update something that doesn't exist.
        }

        // 2. Business Rule: Check if it's already marked as done.
        if (sample.IsTestDone)
        {
            return true; // Already done, so the operation is technically successful.
        }

        // 3. If it exists and is not done, then update it.
        return await sampleStorageRepository.MarkAsDoneAsync(storageId, userId);
    }
}