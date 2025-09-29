using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalMirage.Core.Dtos; // Make sure this is present
using PortalMirage.Business.Abstractions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PortalMirage.Core.Models; // Add this for the 'Role' model

namespace PortalMirage.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
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
        public async Task<ActionResult<IEnumerable<RoleResponse>>> GetAllRoles()
        {
            var roles = await roleService.GetAllRolesAsync();
            var response = roles.Select(r => new RoleResponse(r.RoleID, r.RoleName));
            return Ok(response);
        }

        [HttpGet("users/{username}/roles")]
        public async Task<ActionResult<IEnumerable<RoleResponse>>> GetRolesForUser(string username)
        {
            var roles = await userRoleService.GetRolesForUserAsync(username);
            var response = roles.Select(r => new RoleResponse(r.RoleID, r.RoleName));
            return Ok(response);
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