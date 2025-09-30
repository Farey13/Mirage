using System;

namespace PortalMirage.Core.Dtos;

public record AuditLogDto(
    long AuditID,
    int? UserID,
    string? UserFullName, // Joined from Users table
    DateTime Timestamp,
    string ActionType,
    string? ModuleName,
    string? RecordID,
    string? FieldName,
    string? OldValue,
    string? NewValue
);