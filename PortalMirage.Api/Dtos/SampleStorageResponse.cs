namespace PortalMirage.Api.Dtos;

public record SampleStorageResponse(
    int StorageID,
    string PatientSampleID,
    DateTime StorageDateTime,
    int StoredByUserID,
    bool IsTestDone,
    DateTime? TestDoneDateTime,
    int? TestDoneByUserID);