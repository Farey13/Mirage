using Mirage.UI.ViewModels; // We'll create these DTOs next
using Refit;
using System.Threading.Tasks;

namespace Mirage.UI.Services;

// DTOs for communication
public record LoginRequest(string Username, string Password);
public record UserResponse(int UserId, string Username, string FullName);
public record LoginResponse(string Token, UserResponse User);

public interface IPortalMirageApi
{
    [Post("/api/auth/login")]
    Task<LoginResponse> LoginAsync([Body] LoginRequest loginRequest);
}