using PortalMirage.Core.Models;

namespace Mirage.UI.Services
{
    public interface IAuthService
    {
        string? GetToken();
        void SetToken(string? token);

        User? CurrentUser { get; }

        // --- NEWLY ADDED METHODS ---
        void SetCurrentUser(User user);
        void ClearCurrentUser();
    }
}