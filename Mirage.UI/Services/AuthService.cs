using PortalMirage.Core.Models;
using System;
using System.Linq;
using System.Text.Json; // Required for parsing the Token

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
                // If logging out, clear the user
                CurrentUser = null;
            }
            else
            {
                // If we have a user but no role (or token changed), try to update the role
                if (CurrentUser != null)
                {
                    PopulateRoleFromToken(token);
                }
            }
        }

        public void SetCurrentUser(User user)
        {
            CurrentUser = user;

            // Immediately try to populate the role if we already have a token
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

        // --- HELPER: Extracts Role ("Admin") from the JWT Token ---
        private void PopulateRoleFromToken(string token)
        {
            if (CurrentUser == null) return;

            try
            {
                var role = ExtractClaim(token, "role")
                           ?? ExtractClaim(token, "http://schemas.microsoft.com/ws/2008/06/identity/claims/role");

                if (!string.IsNullOrEmpty(role))
                {
                    CurrentUser.Role = role;
                }
            }
            catch
            {
                // If parsing fails, Role remains null (Safe default)
            }
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
                    // Handle case where user has multiple roles (JSON Array) -> Check if one is Admin
                    if (element.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var item in element.EnumerateArray())
                        {
                            if (item.GetString() == "Admin") return "Admin";
                        }
                        return element.EnumerateArray().FirstOrDefault().GetString();
                    }

                    return element.GetString();
                }
            }
            catch { /* Ignore parsing errors */ }

            return null;
        }
    }
}