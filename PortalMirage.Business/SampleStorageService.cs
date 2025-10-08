using PortalMirage.Business.Abstractions;
using PortalMirage.Core.Models;
using PortalMirage.Data.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortalMirage.Business;

public class SampleStorageService(
    ISampleStorageRepository sampleStorageRepository,
    IAuditLogService auditLogService) : ISampleStorageService
{
    public async Task<SampleStorage> CreateAsync(SampleStorage sampleStorage)
    {
        var newSample = await sampleStorageRepository.CreateAsync(sampleStorage);

        // ADDED: Log the creation event
        await auditLogService.LogAsync(
            userId: newSample.StoredByUserID,
            actionType: "Create",
            moduleName: "SampleStorage",
            recordId: newSample.StorageID.ToString(),
            newValue: $"Sample ID: {newSample.PatientSampleID}, Test: {newSample.TestName}"
        );

        return newSample;
    }

    public async Task<IEnumerable<SampleStorage>> GetPendingByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await sampleStorageRepository.GetPendingByDateRangeAsync(startDate, endDate);
    }

    public async Task<IEnumerable<SampleStorage>> GetCompletedByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await sampleStorageRepository.GetCompletedByDateRangeAsync(startDate, endDate);
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
        var success = await sampleStorageRepository.MarkAsDoneAsync(storageId, userId);
        if (success)
        {
            // ADDED: Log the "test done" event
            await auditLogService.LogAsync(
                userId: userId,
                actionType: "Update",
                moduleName: "SampleStorage",
                recordId: storageId.ToString(),
                newValue: "Marked as Test Done"
            );
        }
        return success;
    }

    public async Task<bool> DeactivateAsync(int storageId, int userId, string reason)
    {
        var success = await sampleStorageRepository.DeactivateAsync(storageId, userId, reason);
        if (success)
        {
            await auditLogService.LogAsync(userId, "Deactivate", "SampleStorage", storageId.ToString(), newValue: reason);
        }
        return success;
    }
}
