using System;
namespace PortalMirage.Core.Dtos;

public record CreateHandoverRequest(string HandoverNotes);
public record HandoverResponse(int HandoverID, string HandoverNotes, DateTime GivenDateTime, int GivenByUserID, bool IsReceived, DateTime? ReceivedDateTime, int? ReceivedByUserID);