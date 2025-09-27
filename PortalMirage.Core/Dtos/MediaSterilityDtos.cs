using System;
namespace PortalMirage.Core.Dtos;

public record CreateMediaSterilityCheckRequest(
    string MediaName,
    string MediaLotNumber,
    string? MediaQuantity,
    string Result37C,
    string Result25C,
    string? Comments);

public record DeactivateMediaSterilityCheckRequest(string Reason);

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
    int PerformedByUserID,
    string PerformedByUsername);