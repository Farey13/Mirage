using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalMirage.Business;
using PortalMirage.Business.Abstractions;
using PortalMirage.Core.Dtos;
using PortalMirage.Core.Models;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace PortalMirage.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;
        private readonly IUserRoleService _userRoleService;
        private readonly IAuditLogService _auditLogService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            IUserService userService,
            IRoleService roleService,
            IUserRoleService userRoleService,
            IAuditLogService auditLogService,
            ILogger<AdminController> logger)
        {
            _userService = userService;
            _roleService = roleService;
            _userRoleService = userRoleService;
            _auditLogService = auditLogService;
            _logger = logger;
        }

        [HttpPost("users/create")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            var actorUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            _logger.LogInformation("Creating user {Username} by admin {AdminUserId}", request.Username, actorUserId);
            
            var newUser = await _userService.RegisterUserAsync(request.Username, request.Password, request.FullName, actorUserId);

            if (newUser is null)
            {
                _logger.LogWarning("Failed to create user - username already taken: {Username}", request.Username);
                return BadRequest("Username is already taken.");
            }

            _logger.LogInformation("User created successfully: {Username}, UserId: {UserId}", request.Username, newUser.UserID);
            var userResponse = new UserResponse(newUser.UserID, newUser.Username, newUser.FullName);
            return CreatedAtAction(nameof(GetAllUsers), new { }, userResponse);
        }

        [HttpGet("users")]
        public async Task<ActionResult<IEnumerable<UserResponse>>> GetAllUsers()
        {
            _logger.LogInformation("Fetching all users");
            var users = await _userService.GetAllUsersAsync();
            var response = users.Select(u => new UserResponse(u.UserID, u.Username, u.FullName));
            return Ok(response);
        }

        [HttpGet("roles")]
        public async Task<ActionResult<IEnumerable<RoleResponse>>> GetAllRoles()
        {
            _logger.LogInformation("Fetching all roles");
            var roles = await _roleService.GetAllRolesAsync();
            var response = roles.Select(r => new RoleResponse(r.RoleID, r.RoleName));
            return Ok(response);
        }

        [HttpPost("roles")]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest request)
        {
            _logger.LogInformation("Creating role: {RoleName}", request.RoleName);
            var newRole = await _roleService.CreateRoleAsync(request.RoleName);
            if (newRole is null)
            {
                _logger.LogWarning("Failed to create role - already exists: {RoleName}", request.RoleName);
                return BadRequest("A role with this name already exists.");
            }
            var response = new RoleResponse(newRole.RoleID, newRole.RoleName);
            return CreatedAtAction(nameof(GetAllRoles), response);
        }

        [HttpGet("users/{username}/roles")]
        public async Task<ActionResult<IEnumerable<RoleResponse>>> GetRolesForUser(string username)
        {
            _logger.LogInformation("Fetching roles for user: {Username}", username);
            var roles = await _userRoleService.GetRolesForUserAsync(username);
            var response = roles.Select(r => new RoleResponse(r.RoleID, r.RoleName));
            return Ok(response);
        }

        [HttpPost("users/reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var actorUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            _logger.LogInformation("Resetting password for user {Username} by admin {AdminUserId}", request.Username, actorUserId);
            
            var success = await _userService.ResetPasswordAsync(request.Username, request.NewPassword, actorUserId);

            if (!success)
            {
                _logger.LogWarning("Failed to reset password - user not found: {Username}", request.Username);
                return NotFound("User not found.");
            }

            return Ok("Password has been reset successfully.");
        }

        [HttpPost("assign-role")]
        public async Task<IActionResult> AssignRole([FromBody] AssignRoleRequest request)
        {
            var actorUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            _logger.LogInformation("Assigning role {RoleName} to user {Username} by admin {AdminUserId}", 
                request.RoleName, request.Username, actorUserId);
            
            var success = await _userRoleService.AssignRoleToUserAsync(request.Username, request.RoleName, actorUserId);

            if (!success)
            {
                _logger.LogWarning("Failed to assign role {RoleName} to user {Username}", request.RoleName, request.Username);
                return BadRequest("Failed to assign role. Check if user and role exist.");
            }

            return Ok("Role assigned successfully.");
        }

        [HttpPost("remove-role")]
        public async Task<IActionResult> RemoveRoleFromUser([FromBody] AssignRoleRequest request)
        {
            var actorUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            _logger.LogInformation("Removing role {RoleName} from user {Username} by admin {AdminUserId}", 
                request.RoleName, request.Username, actorUserId);
            
            var success = await _userRoleService.RemoveRoleFromUserAsync(request.Username, request.RoleName, actorUserId);
            if (!success)
            {
                _logger.LogWarning("Failed to remove role {RoleName} from user {Username}", request.RoleName, request.Username);
                return BadRequest("Failed to remove role from user.");
            }
            return Ok("Role removed successfully.");
        }
    }
}