namespace PortalMirage.Core.Dtos;

public record AdminListItemDto(int ItemID, string ListType, string ItemValue, string? Description, bool IsActive);
public record CreateAdminListItemRequest(string ListType, string ItemValue, string? Description);
public record UpdateAdminListItemRequest(int ItemID, string ItemValue, string? Description, bool IsActive);