using PortalMirage.Business.Abstractions;
using PortalMirage.Core.Models;
using PortalMirage.Data.Abstractions;
using System.Collections.Generic;
using Task = System.Threading.Tasks.Task;

namespace PortalMirage.Business;

public class UserService(IUserRepository userRepository, IAuditLogService auditLogService) : IUserService
{
    public async Task<User?> RegisterUserAsync(string username, string password, string fullName, int? actorUserId)
    {
        var existingUser = await userRepository.GetByUsernameAsync(username);
        if (existingUser is not null)
        {
            return null;
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

        var userToCreate = new User
        {
            Username = username,
            PasswordHash = passwordHash,
            FullName = fullName,
            IsActive = true
        };

        var createdUser = await userRepository.CreateAsync(userToCreate);

        // Only log if an actor ID is provided (i.e., an admin is creating the user)
        if (actorUserId.HasValue)
        {
            await auditLogService.LogAsync(actorUserId.Value, "Create", "UserManagement", createdUser.UserID.ToString(), newValue: $"Created new user '{createdUser.Username}'");
        }

        return createdUser;
    }

    public async Task<User?> ValidateCredentialsAsync(string username, string password)
    {
        var user = await userRepository.GetByUsernameAsync(username);
        if (user is null)
        {
            return null;
        }

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            return null;
        }

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

    public async Task<bool> ResetPasswordAsync(string username, string newPassword, int actorUserId)
    {
        var user = await userRepository.GetByUsernameAsync(username);
        if (user is null)
        {
            return false;
        }

        var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        var success = await userRepository.UpdatePasswordHashAsync(user.UserID, newPasswordHash);

        if (success)
        {
            // Correctly logs the ID of the admin who performed the action
            await auditLogService.LogAsync(
                userId: actorUserId,
                actionType: "ResetPassword",
                moduleName: "UserManagement",
                recordId: user.UserID.ToString(),
                newValue: $"Password reset for user '{username}'."
            );
        }
        return success;
    }
}