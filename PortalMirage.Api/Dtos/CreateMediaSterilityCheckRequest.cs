namespace PortalMirage.Api.Dtos;

public record CreateMediaSterilityCheckRequest(
    string MediaName,
    string MediaLotNumber,
    string? MediaQuantity,
    string Result37C, // Should be "No Growth" or "Growth Seen"
    string Result25C, // Should be "No Growth" or "Growth Seen"
    string? Comments);