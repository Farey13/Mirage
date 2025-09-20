using PortalMirage.Business.Abstractions;
using PortalMirage.Core.Models;
using PortalMirage.Data.Abstractions;
using Task = System.Threading.Tasks.Task;

namespace PortalMirage.Business;

public class UserService(IUserRepository userRepository) : IUserService
{
    public async Task<User?> RegisterUserAsync(string username, string password, string fullName)
    {
        // 1. Business Rule: Check if the username already exists
        var existingUser = await userRepository.GetByUsernameAsync(username);
        if (existingUser is not null)
        {
            // Username is already taken, registration fails.
            return null;
        }

        // 2. Business Rule: Hash the password before storing it
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

        // 3. Create the new user object
        var userToCreate = new User
        {
            Username = username,
            PasswordHash = passwordHash,
            FullName = fullName,
            IsActive = true // Default to active
        };

        // 4. Pass the new user to the data layer to be saved
        var createdUser = await userRepository.CreateAsync(userToCreate);

        return createdUser;
    }
}