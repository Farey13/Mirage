using PortalMirage.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortalMirage.Business.Abstractions;

public interface IUserService
{
    Task<User?> RegisterUserAsync(string username, string password, string fullName, int? actorUserId); // Add actorUserId
    Task<User?> ValidateCredentialsAsync(string username, string password);
    Task<IEnumerable<User>> GetAllUsersAsync();
    Task<User?> GetUserByIdAsync(int userId);
    Task<bool> ResetPasswordAsync(string username, string newPassword, int actorUserId); // Add actorUserId
}