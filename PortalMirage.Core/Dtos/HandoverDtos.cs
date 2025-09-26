using System;

namespace PortalMirage.Core.Dtos;

public record CreateHandoverRequest(string HandoverNotes, string Priority, string Shift);

public record DeactivateHandoverRequest(string Reason);
public record HandoverResponse(
    int HandoverID,
    string HandoverNotes,
    string Priority,
    string Shift,
    DateTime GivenDateTime,
    int GivenByUserID,
    string GivenByUsername,
    bool IsReceived,
    DateTime? ReceivedDateTime,
    int? ReceivedByUserID,
    string? ReceivedByUsername);