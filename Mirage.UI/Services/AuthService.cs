using PortalMirage.Core.Models;

namespace Mirage.UI.Services
{
    public class AuthService : IAuthService
    {
        private string? _token;

        // Add this property
        public User? CurrentUser { get; private set; }

        public string? GetToken()
        {
            return _token;
        }

        public void SetToken(string? token)
        {
            _token = token;

            // Optional: When token is set, you might want to clear CurrentUser
            // if the token is being cleared (logged out)
            if (string.IsNullOrEmpty(token))
            {
                CurrentUser = null;
            }
        }

        // Optional: Add methods to set and clear CurrentUser
        public void SetCurrentUser(User user)
        {
            CurrentUser = user;
        }

        public void ClearCurrentUser()
        {
            CurrentUser = null;
        }
    }
}