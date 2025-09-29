namespace PortalMirage.Core.Dtos;

// This is the missing class causing the error
public record AssignRoleRequest(string Username, string RoleName);



// This is used by your GetAllRoles endpoint
public record RoleResponse(int RoleID, string RoleName);