namespace PortalMirage.Api.Dtos;

public record KitValidationResponse(
    int ValidationID,
    string KitName,
    string KitLotNumber,
    DateTime KitExpiryDate,
    string ValidationStatus,
    string? Comments,
    DateTime ValidationDateTime,
    int ValidatedByUserID);