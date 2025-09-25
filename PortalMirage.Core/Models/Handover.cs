namespace PortalMirage.Core.Models;

public record Handover
{
    public int HandoverID { get; init; }
    public required string HandoverNotes { get; set; }
    public DateTime GivenDateTime { get; init; }
    public int GivenByUserID { get; set; }
    public bool IsReceived { get; set; }
    public DateTime? ReceivedDateTime { get; set; }
    public int? ReceivedByUserID { get; set; }

    public required string Priority { get; set; } // Add this
    public required string Shift { get; set; }    // Add this
}