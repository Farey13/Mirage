using PortalMirage.Business.Abstractions;
using PortalMirage.Core.Models;
using PortalMirage.Data.Abstractions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortalMirage.Business;

public class SampleStorageService : ISampleStorageService
{
    private readonly ISampleStorageRepository _sampleStorageRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<SampleStorageService> _logger;

    public SampleStorageService(
        ISampleStorageRepository sampleStorageRepository,
        IAuditLogService auditLogService,
        ILogger<SampleStorageService> logger)
    {
        _sampleStorageRepository = sampleStorageRepository;
        _auditLogService = auditLogService;
        _logger = logger;
    }

    public async Task<SampleStorage> CreateAsync(SampleStorage sampleStorage)
    {
        _logger.LogInformation("Creating sample storage for PatientSampleID: {PatientSampleID}", sampleStorage.PatientSampleID);
        var newSample = await _sampleStorageRepository.CreateAsync(sampleStorage);

        await _auditLogService.LogAsync(
            userId: newSample.StoredByUserID,
            actionType: "Create",
            moduleName: "SampleStorage",
            recordId: newSample.StorageID.ToString(),
            newValue: $"Sample ID: {newSample.PatientSampleID}, Test: {newSample.TestName}"
        );

        _logger.LogInformation("Sample storage created with ID: {StorageId}", newSample.StorageID);
        return newSample;
    }

    public async Task<IEnumerable<SampleStorage>> GetPendingByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        _logger.LogDebug("Fetching pending sample storage from {StartDate} to {EndDate}", startDate, endDate);
        return await _sampleStorageRepository.GetPendingByDateRangeAsync(startDate, endDate);
    }

    public async Task<IEnumerable<SampleStorage>> GetCompletedByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        _logger.LogDebug("Fetching completed sample storage from {StartDate} to {EndDate}", startDate, endDate);
        return await _sampleStorageRepository.GetCompletedByDateRangeAsync(startDate, endDate);
    }

    public async Task<bool> MarkAsDoneAsync(int storageId, int userId)
    {
        _logger.LogInformation("Marking sample storage as done: {StorageId} by user {UserId}", storageId, userId);
        
        var sample = await _sampleStorageRepository.GetByIdAsync(storageId);
        if (sample is null)
        {
            _logger.LogWarning("Sample storage not found: {StorageId}", storageId);
            return false;
        }

        if (sample.IsTestDone)
        {
            _logger.LogInformation("Sample storage already marked as done: {StorageId}", storageId);
            return true;
        }

        var success = await _sampleStorageRepository.MarkAsDoneAsync(storageId, userId);
        if (success)
        {
            await _auditLogService.LogAsync(
                userId: userId,
                actionType: "Update",
                moduleName: "SampleStorage",
                recordId: storageId.ToString(),
                newValue: "Marked as Test Done"
            );
            _logger.LogInformation("Sample storage {StorageId} marked as done", storageId);
        }
        return success;
    }

    public async Task<bool> DeactivateAsync(int storageId, int userId, string reason)
    {
        _logger.LogInformation("Deactivating sample storage {StorageId} by user {UserId}", storageId, userId);
        var success = await _sampleStorageRepository.DeactivateAsync(storageId, userId, reason);
        if (success)
        {
            await _auditLogService.LogAsync(userId, "Deactivate", "SampleStorage", storageId.ToString(), newValue: reason);
            _logger.LogInformation("Sample storage {StorageId} deactivated successfully", storageId);
        }
        return success;
    }
}
