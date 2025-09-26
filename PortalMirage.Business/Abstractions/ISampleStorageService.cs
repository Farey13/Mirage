using PortalMirage.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortalMirage.Business.Abstractions;

public interface ISampleStorageService
{
    Task<SampleStorage> CreateAsync(SampleStorage sampleStorage);
    Task<IEnumerable<SampleStorage>> GetPendingByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<SampleStorage>> GetCompletedByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<bool> MarkAsDoneAsync(int storageId, int userId);
    Task<bool> DeactivateAsync(int storageId, int userId, string reason); // This is the corrected signature
}