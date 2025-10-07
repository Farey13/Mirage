using PortalMirage.Core.Dtos;
using PortalMirage.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortalMirage.Data.Abstractions;

public interface ISampleStorageRepository
{
    Task<SampleStorage> CreateAsync(SampleStorage sampleStorage);
    Task<IEnumerable<SampleStorage>> GetPendingByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<SampleStorage>> GetCompletedByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<SampleStorage?> GetByIdAsync(int storageId);
    Task<bool> MarkAsDoneAsync(int storageId, int userId);
    Task<bool> DeactivateAsync(int storageId, int userId, string reason); // This is the corrected signature

    Task<int> GetPendingCountAsync(); // ADD THIS LINE

    Task<IEnumerable<SampleStorageReportDto>> GetReportDataAsync(DateTime startDate, DateTime endDate, string? testName, string? status);
}