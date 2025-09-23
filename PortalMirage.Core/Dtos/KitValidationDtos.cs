using System;
namespace PortalMirage.Core.Dtos;

public record CreateKitValidationRequest(string KitName, string KitLotNumber, DateTime KitExpiryDate, string ValidationStatus, string? Comments);
public record KitValidationResponse(int ValidationID, string KitName, string KitLotNumber, DateTime KitExpiryDate, string ValidationStatus, string? Comments, DateTime ValidationDateTime, int ValidatedByUserID, string ValidatedByUsername);