namespace Mirage.UI.Services
{
    public class AuthService : IAuthService
    {
        private string? _token;

        public string? GetToken()
        {
            return _token;
        }

        public void SetToken(string? token)
        {
            _token = token;
        }
    }
}