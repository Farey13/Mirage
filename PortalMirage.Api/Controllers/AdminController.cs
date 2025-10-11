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

namespace PortalMirage.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")] // Secure the entire controller for Admins
    public class AdminController(
        IUserService userService,
        IRoleService roleService,
        IUserRoleService userRoleService,
        IAuditLogService auditLogService) : ControllerBase
    {
        private readonly IAuditLogService _auditLogService = auditLogService;

        [HttpPost("users/create")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            var actorUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var newUser = await userService.RegisterUserAsync(request.Username, request.Password, request.FullName, actorUserId);

            if (newUser is null)
            {
                return BadRequest("Username is already taken.");
            }

            var userResponse = new UserResponse(newUser.UserID, newUser.Username, newUser.FullName);
            return CreatedAtAction(nameof(GetAllUsers), new { }, userResponse);
        }

        [HttpGet("users")]
        public async Task<ActionResult<IEnumerable<UserResponse>>> GetAllUsers()
        {
            var users = await userService.GetAllUsersAsync();
            var response = users.Select(u => new UserResponse(u.UserID, u.Username, u.FullName));
            return Ok(response);
        }

        [HttpGet("roles")]
        public async Task<ActionResult<IEnumerable<RoleResponse>>> GetAllRoles()
        {
            var roles = await roleService.GetAllRolesAsync();
            var response = roles.Select(r => new RoleResponse(r.RoleID, r.RoleName));
            return Ok(response);
        }

        [HttpPost("roles")] // NEW ENDPOINT
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest request)
        {
            var newRole = await roleService.CreateRoleAsync(request.RoleName);
            if (newRole is null)
            {
                return BadRequest("A role with this name already exists.");
            }
            var response = new RoleResponse(newRole.RoleID, newRole.RoleName);
            return CreatedAtAction(nameof(GetAllRoles), response);
        }

        [HttpGet("users/{username}/roles")]
        public async Task<ActionResult<IEnumerable<RoleResponse>>> GetRolesForUser(string username)
        {
            var roles = await userRoleService.GetRolesForUserAsync(username);
            var response = roles.Select(r => new RoleResponse(r.RoleID, r.RoleName));
            return Ok(response);
        }

        [HttpPost("users/reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var actorUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var success = await userService.ResetPasswordAsync(request.Username, request.NewPassword, actorUserId);

            if (!success)
            {
                return NotFound("User not found.");
            }

            // The audit log is now handled by the service, so we don't need to log it here.
            return Ok("Password has been reset successfully.");
        }

        [HttpPost("assign-role")]
        public async Task<IActionResult> AssignRole([FromBody] AssignRoleRequest request)
        {
            var actorUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var success = await userRoleService.AssignRoleToUserAsync(request.Username, request.RoleName, actorUserId);

            if (!success)
            {
                return BadRequest("Failed to assign role. Check if user and role exist.");
            }

            return Ok("Role assigned successfully.");
        }

        [HttpPost("remove-role")]
        public async Task<IActionResult> RemoveRoleFromUser([FromBody] AssignRoleRequest request)
        {
            var actorUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var success = await userRoleService.RemoveRoleFromUserAsync(request.Username, request.RoleName, actorUserId);
            if (!success)
            {
                return BadRequest("Failed to remove role from user.");
            }
            return Ok("Role removed successfully.");
        }
    }
}