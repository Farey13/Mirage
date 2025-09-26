using System;

namespace PortalMirage.Core.Dtos;

public record CreateSampleStorageRequest(string PatientSampleID, string TestName);

public record DeactivateSampleStorageRequest(string Reason);

public record SampleStorageResponse(
    int StorageID,
    string PatientSampleID,
    string TestName,
    DateTime StorageDateTime,
    int StoredByUserID,
    string StoredByUsername,
    bool IsTestDone,
    DateTime? TestDoneDateTime,
    int? TestDoneByUserID,
    string? TestDoneByUsername); // Add this nullable property