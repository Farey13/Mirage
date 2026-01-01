namespace PortalMirage.Core.Models;

public record User
{
    public int UserID { get; init; }
    public required string Username { get; set; }
    public required string PasswordHash { get; set; }
    public required string FullName { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; init; }

    // --- NEW ADDITION ---
    // This property holds the role from the Token (JWT) for the UI to use.
    // It is not stored in the 'Users' table, so it won't break your SQL INSERTs.
    public string? Role { get; set; }
}