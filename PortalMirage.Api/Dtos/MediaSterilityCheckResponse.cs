namespace PortalMirage.Api.Dtos;

public record MediaSterilityCheckResponse(
    int SterilityCheckID,
    string MediaName,
    string MediaLotNumber,
    string? MediaQuantity,
    string Result37C,
    string Result25C,
    string OverallStatus,
    string? Comments,
    DateTime CheckDateTime,
    int PerformedByUserID);