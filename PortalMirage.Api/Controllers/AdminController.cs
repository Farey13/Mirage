using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalMirage.Api.Dtos;
using PortalMirage.Business.Abstractions;
using PortalMirage.Core.Models;

namespace PortalMirage.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // IMPORTANT: This locks the entire controller
    public class AdminController(
        IUserService userService,
        IRoleService roleService,
        IUserRoleService userRoleService) : ControllerBase
    {
        [HttpGet("users")]
        public async Task<ActionResult<IEnumerable<UserResponse>>> GetAllUsers()
        {
            var users = await userService.GetAllUsersAsync();
            var response = users.Select(u => new UserResponse(u.UserID, u.Username, u.FullName));
            return Ok(response);
        }

        [HttpGet("roles")]
        public async Task<ActionResult<IEnumerable<Role>>> GetAllRoles()
        {
            var roles = await roleService.GetAllRolesAsync();
            return Ok(roles);
        }

        [HttpPost("assign-role")]
        public async Task<IActionResult> AssignRole([FromBody] AssignRoleRequest request)
        {
            var success = await userRoleService.AssignRoleToUserAsync(request.Username, request.RoleName);

            if (!success)
            {
                return BadRequest("Failed to assign role. Check if user and role exist.");
            }

            return Ok("Role assigned successfully.");
        }
    }
}