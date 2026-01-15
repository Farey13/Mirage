using PortalMirage.Core.Models;
using System;
using System.Linq;
using System.Text.Json;

namespace Mirage.UI.Services
{
    public class AuthService : IAuthService
    {
        private string? _token;
        public User? CurrentUser { get; private set; }

        public string? GetToken() => _token;

        public void SetToken(string? token)
        {
            _token = token;

            if (string.IsNullOrEmpty(token))
            {
                CurrentUser = null;
            }
            else
            {
                // FIX: If we have a token but no user (e.g. App Restart), Re-hydrate the User!
                if (CurrentUser == null)
                {
                    HydrateUserFromToken(token);
                }

                // Always try to update the role from the token
                PopulateRoleFromToken(token);
            }
        }

        public void SetCurrentUser(User user)
        {
            CurrentUser = user;
            if (!string.IsNullOrEmpty(_token))
            {
                PopulateRoleFromToken(_token);
            }
        }

        public void ClearCurrentUser()
        {
            CurrentUser = null;
            _token = null;
        }

        private void HydrateUserFromToken(string token)
        {
            try
            {
                // Extract basics to keep the app running
                var id = ExtractClaim(token, "nameid") ?? ExtractClaim(token, "sub");
                var name = ExtractClaim(token, "unique_name") ?? ExtractClaim(token, "name") ?? "Unknown";

                if (int.TryParse(id, out int userId))
                {
                    // Create a "Placeholder" user so the UI doesn't crash
                    CurrentUser = new User
                    {
                        UserID = userId,
                        Username = name,
                        FullName = name,
                        PasswordHash = "", // Dummy value (not needed on UI)
                        IsActive = true
                    };
                }
            }
            catch { /* Ignore */ }
        }

        private void PopulateRoleFromToken(string token)
        {
            // Now CurrentUser should not be null because we called HydrateUserFromToken above
            if (CurrentUser == null) return;

            try
            {
                var role = ExtractClaim(token, "role")
                           ?? ExtractClaim(token, "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")
                           ?? ExtractClaim(token, "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/role");

                if (!string.IsNullOrEmpty(role))
                {
                    if (string.Equals(role, "admin", StringComparison.OrdinalIgnoreCase))
                        CurrentUser.Role = "Admin";
                    else
                        CurrentUser.Role = role;
                }
            }
            catch { }
        }

        private string? ExtractClaim(string token, string claimType)
        {
            try
            {
                var parts = token.Split('.');
                if (parts.Length < 2) return null;

                var payload = parts[1];
                switch (payload.Length % 4)
                {
                    case 2: payload += "=="; break;
                    case 3: payload += "="; break;
                }

                var jsonBytes = Convert.FromBase64String(payload.Replace('-', '+').Replace('_', '/'));
                using var doc = JsonDocument.Parse(jsonBytes);
                var root = doc.RootElement;

                if (root.TryGetProperty(claimType, out var element))
                {
                    if (element.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var item in element.EnumerateArray())
                        {
                            if (string.Equals(item.GetString(), "Admin", StringComparison.OrdinalIgnoreCase)) return "Admin";
                        }
                        return element.EnumerateArray().FirstOrDefault().GetString();
                    }
                    return element.GetString();
                }
            }
            catch { }
            return null;
        }
    }
}