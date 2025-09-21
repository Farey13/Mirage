namespace PortalMirage.Core.Models;

public record User
{
    public int UserID { get; init; }
    public required string Username { get; set; }
    public required string PasswordHash { get; set; }
    public required string FullName { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; init; }
}