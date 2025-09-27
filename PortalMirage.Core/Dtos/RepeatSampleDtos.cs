using System;

namespace PortalMirage.Core.Dtos;

public record CreateRepeatSampleRequest(
    string PatientIdCardNumber,
    string PatientName,
    string? ReasonText,
    string? InformedPerson,
    string? Department);

public record DeactivateRepeatSampleRequest(string Reason);

public record RepeatSampleResponse(
    int RepeatID,
    string? PatientIdCardNumber,
    string PatientName,
    string? ReasonText,
    string? InformedPerson,
    string? Department,
    DateTime LogDateTime,
    int LoggedByUserID,
    string LoggedByUsername);