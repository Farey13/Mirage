namespace PortalMirage.Core.Models;

public record SampleStorage
{
    public int StorageID { get; init; }
    public required string PatientSampleID { get; set; }

    public required string TestName { get; set; } // Add this line
    public DateTime StorageDateTime { get; init; }
    public int StoredByUserID { get; set; }
    public bool IsTestDone { get; set; }
    public DateTime? TestDoneDateTime { get; set; }
    public int? TestDoneByUserID { get; set; }
}