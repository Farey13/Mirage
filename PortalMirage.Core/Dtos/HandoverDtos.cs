// Replace the entire file content with this
using System;

namespace PortalMirage.Core.Dtos;

public record CreateHandoverRequest(string HandoverNotes, string Priority, string Shift);

public record HandoverResponse(
    int HandoverID,
    string HandoverNotes,
    string Priority,
    string Shift,
    DateTime GivenDateTime,
    int GivenByUserID,
    string GivenByUsername, // Added for username
    bool IsReceived,
    DateTime? ReceivedDateTime,
    int? ReceivedByUserID,
    string? ReceivedByUsername); // Added for username