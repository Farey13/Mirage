using PortalMirage.Core.Models;

namespace Mirage.UI.Services
{
    public interface IAuthService
    {
        string? GetToken();
        void SetToken(string? token);

        // Add this property
        User? CurrentUser { get; }
    }
}