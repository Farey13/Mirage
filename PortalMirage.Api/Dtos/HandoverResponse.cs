namespace PortalMirage.Api.Dtos;

public record HandoverResponse(
    int HandoverID,
    string HandoverNotes,
    DateTime GivenDateTime,
    int GivenByUserID,
    bool IsReceived,
    DateTime? ReceivedDateTime,
    int? ReceivedByUserID);