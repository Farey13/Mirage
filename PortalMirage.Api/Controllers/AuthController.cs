using Microsoft.AspNetCore.Mvc;
using PortalMirage.Core.Dtos;
using PortalMirage.Business.Abstractions;
using Microsoft.Extensions.Logging;
using Task = System.Threading.Tasks.Task;


namespace PortalMirage.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(
        IUserService userService, 
        IJwtTokenGenerator jwtTokenGenerator,
        ILogger<AuthController> logger) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserRequest request)
        {
            logger.LogInformation("Registration attempt for username: {Username}", request.Username);
            var newUser = await userService.RegisterUserAsync(request.Username, request.Password, request.FullName, null);

            if (newUser is null)
            {
                logger.LogWarning("Registration failed - username already taken: {Username}", request.Username);
                return BadRequest("Username is already taken.");
            }

            logger.LogInformation("User registered successfully: {Username}, UserId: {UserId}", request.Username, newUser.UserID);
            var userResponse = new UserResponse(newUser.UserID, newUser.Username, newUser.FullName);
            return Ok(userResponse);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            logger.LogInformation("Login attempt for username: {Username}", request.Username);
            var user = await userService.ValidateCredentialsAsync(request.Username, request.Password);

            if (user is null)
            {
                logger.LogWarning("Login failed - invalid credentials for username: {Username}", request.Username);
                return Unauthorized("Invalid username or password.");
            }

            var token = await jwtTokenGenerator.GenerateTokenAsync(user);
            var userResponse = new UserResponse(user.UserID, user.Username, user.FullName);

            logger.LogInformation("User logged in successfully: {Username}, UserId: {UserId}", request.Username, user.UserID);
            return Ok(new LoginResponse(token, userResponse));
        }
        
    }
}