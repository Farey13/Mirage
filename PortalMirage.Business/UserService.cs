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

    public async Task<User?> ValidateCredentialsAsync(string username, string password)
    {
        // 1. Find the user by their username
        var user = await userRepository.GetByUsernameAsync(username);
        if (user is null)
        {
            return null; // User not found
        }

        // 2. Verify the provided password against the stored hash
        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            return null; // Password does not match
        }

        // 3. If validation succeeds, return the user object
        return user;
    }
    public async Task<User?> GetUserByIdAsync(int userId)
    {
        return await userRepository.GetByIdAsync(userId);
    }
    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        return await userRepository.GetAllAsync();
    }
}