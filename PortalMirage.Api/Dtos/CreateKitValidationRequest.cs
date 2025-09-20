namespace PortalMirage.Api.Dtos;

public record CreateKitValidationRequest(
    string KitName,
    string KitLotNumber,
    DateTime KitExpiryDate,
    string ValidationStatus,
    string? Comments);