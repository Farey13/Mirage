namespace PortalMirage.Core.Dtos;

public record LoginRequest(string Username, string Password);
public record UserResponse(int UserId, string Username, string FullName);
public record LoginResponse(string Token, UserResponse User);
public record RegisterUserRequest(string Username, string Password, string FullName);