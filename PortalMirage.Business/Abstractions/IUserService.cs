using PortalMirage.Core.Models;

namespace PortalMirage.Business.Abstractions;

public interface IUserService
{
    Task<User?> RegisterUserAsync(string username, string password, string fullName);
}