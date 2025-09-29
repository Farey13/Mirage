namespace PortalMirage.Core.Models;

public record AdminListItem
{
    public int ItemID { get; init; }
    public required string ListType { get; set; }
    public required string ItemValue { get; set; }
    public bool IsActive { get; set; }

    public string? Description { get; set; } // Add this line
}