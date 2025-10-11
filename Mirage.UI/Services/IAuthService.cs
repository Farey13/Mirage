namespace Mirage.UI.Services
{
    public interface IAuthService
    {
        string? GetToken();
        void SetToken(string? token);
    }
}