namespace PortalMirage.Core.Models;

public record AuditLog
{
    public long AuditID { get; init; }
    public int? UserID { get; set; } // Nullable, as some system events might not have a user
    public DateTime Timestamp { get; init; }
    public required string ActionType { get; set; }
    public string? ModuleName { get; set; }
    public string? RecordID { get; set; }
    public string? FieldName { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
}