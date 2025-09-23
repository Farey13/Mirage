using System;
namespace PortalMirage.Core.Dtos;

public record CreateSampleStorageRequest(string PatientSampleID);
public record SampleStorageResponse(int StorageID, string PatientSampleID, DateTime StorageDateTime, int StoredByUserID, bool IsTestDone, DateTime? TestDoneDateTime, int? TestDoneByUserID);