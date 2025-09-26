namespace PortalMirage.Core.Models;

public record KitValidation
{
    public int ValidationID { get; init; }
    public required string KitName { get; set; }
    public required string KitLotNumber { get; set; }
    public DateTime KitExpiryDate { get; set; }
    public required string ValidationStatus { get; set; }
    public string? Comments { get; set; }
    public DateTime ValidationDateTime { get; init; }
    public int ValidatedByUserID { get; set; }

    public bool IsActive { get; set; }
    public string? DeactivationReason { get; set; }
    public int? DeactivatedByUserID { get; set; }
    public DateTime? DeactivationDateTime { get; set; }
}