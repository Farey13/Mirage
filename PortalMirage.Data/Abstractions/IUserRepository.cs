using PortalMirage.Core.Models;

namespace PortalMirage.Data.Abstractions;

public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string username);
    Task<User> CreateAsync(User user);

    Task<IEnumerable<User>> GetAllAsync(); // Add this line
}