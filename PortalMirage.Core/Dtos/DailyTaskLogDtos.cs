namespace PortalMirage.Core.Dtos;

public record UpdateTaskStatusRequest(string Status);
// The response for the daily task log is the TaskLogDetailDto, which is already correctly located in PortalMirage.Core/Models.