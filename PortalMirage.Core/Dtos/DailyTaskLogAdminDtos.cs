using System;
namespace PortalMirage.Core.Dtos;
public record ExtendTaskDeadlineRequest(DateTime NewDeadline, string Reason);