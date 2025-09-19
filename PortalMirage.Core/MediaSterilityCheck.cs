namespace PortalMirage.Core.Models;

public record MediaSterilityCheck
{
    public int SterilityCheckID { get; init; }
    public required string MediaName { get; set; }
    public required string MediaLotNumber { get; set; }
    public string? MediaQuantity { get; set; }
    public required string Result37C { get; set; }
    public required string Result25C { get; set; }
    public required string OverallStatus { get; set; }
    public string? Comments { get; set; }
    public DateTime CheckDateTime { get; init; }
    public int PerformedByUserID { get; set; }
}