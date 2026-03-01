using PortalMirage.Business.Abstractions;
using PortalMirage.Core.Models;
using PortalMirage.Data.Abstractions;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Task = System.Threading.Tasks.Task;

namespace PortalMirage.Business;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserRepository userRepository, 
        IAuditLogService auditLogService,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _auditLogService = auditLogService;
        _logger = logger;
    }

    public async Task<User?> RegisterUserAsync(string username, string password, string fullName, int? actorUserId)
    {
        _logger.LogInformation("Registering new user: {Username}", username);
        
        var existingUser = await _userRepository.GetByUsernameAsync(username);
        if (existingUser is not null)
        {
            _logger.LogWarning("Registration failed - username already exists: {Username}", username);
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

        var createdUser = await _userRepository.CreateAsync(userToCreate);

        if (actorUserId.HasValue)
        {
            await _auditLogService.LogAsync(actorUserId.Value, "Create", "UserManagement", createdUser.UserID.ToString(), newValue: $"Created new user '{createdUser.Username}'");
        }

        _logger.LogInformation("User registered successfully: {Username}, UserId: {UserId}", username, createdUser.UserID);
        return createdUser;
    }

    public async Task<User?> ValidateCredentialsAsync(string username, string password)
    {
        _logger.LogDebug("Validating credentials for user: {Username}", username);
        
        var user = await _userRepository.GetByUsernameAsync(username);
        if (user is null)
        {
            _logger.LogWarning("Login failed - user not found: {Username}", username);
            return null;
        }

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            _logger.LogWarning("Login failed - invalid password for user: {Username}", username);
            return null;
        }

        _logger.LogInformation("User validated successfully: {Username}, UserId: {UserId}", username, user.UserID);
        return user;
    }

    public async Task<User?> GetUserByIdAsync(int userId)
    {
        return await _userRepository.GetByIdAsync(userId);
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        _logger.LogDebug("Fetching all users");
        return await _userRepository.GetAllAsync();
    }

    public async Task<bool> ResetPasswordAsync(string username, string newPassword, int actorUserId)
    {
        _logger.LogInformation("Resetting password for user: {Username} by admin: {AdminUserId}", username, actorUserId);
        
        var user = await _userRepository.GetByUsernameAsync(username);
        if (user is null)
        {
            _logger.LogWarning("Password reset failed - user not found: {Username}", username);
            return false;
        }

        var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        var success = await _userRepository.UpdatePasswordHashAsync(user.UserID, newPasswordHash);

        if (success)
        {
            await _auditLogService.LogAsync(
                userId: actorUserId,
                actionType: "ResetPassword",
                moduleName: "UserManagement",
                recordId: user.UserID.ToString(),
                newValue: $"Password reset for user '{username}'."
            );
            _logger.LogInformation("Password reset successfully for user: {Username}", username);
        }
        return success;
    }
}