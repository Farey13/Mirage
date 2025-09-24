using PortalMirage.Core.Models;

namespace PortalMirage.Business.Abstractions;

public interface IUserService
{
    Task<User?> RegisterUserAsync(string username, string password, string fullName);
    Task<User?> ValidateCredentialsAsync(string username, string password); // Add this line
    Task<IEnumerable<User>> GetAllUsersAsync(); // Add this line

    Task<User?> GetUserByIdAsync(int userId);
}