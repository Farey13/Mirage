namespace PortalMirage.Api.Dtos;

public record LoginResponse(string Token, UserResponse User);