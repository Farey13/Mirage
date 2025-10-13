using Microsoft.AspNetCore.Mvc;
using PortalMirage.Core.Dtos; // This is the new, correct location
using PortalMirage.Business.Abstractions;
using Task = System.Threading.Tasks.Task;


namespace PortalMirage.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(IUserService userService, IJwtTokenGenerator jwtTokenGenerator) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserRequest request)
        {
            var newUser = await userService.RegisterUserAsync(request.Username, request.Password, request.FullName, null);

            if (newUser is null)
            {
                return BadRequest("Username is already taken.");
            }

            var userResponse = new UserResponse(newUser.UserID, newUser.Username, newUser.FullName);
            return Ok(userResponse);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await userService.ValidateCredentialsAsync(request.Username, request.Password);

            if (user is null)
            {
                return Unauthorized("Invalid username or password.");
            }

            // Call the new async method with 'await'
            var token = await jwtTokenGenerator.GenerateTokenAsync(user);
            var userResponse = new UserResponse(user.UserID, user.Username, user.FullName);

            return Ok(new LoginResponse(token, userResponse));
        }
        
    }
}