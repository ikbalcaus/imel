using Imel.Interfaces;
using Imel.Models;
using Imel.Models.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Imel.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest loginRequest)
        {
            var result = _authService.ValidateUser(loginRequest);

            if (!result.Success)
            {
                _logger.LogWarning($"Failed login attempt for {loginRequest.Email}. Remaining attempts: ");
                return Unauthorized(result);
            }

            var tokenResponse = _authService.GenerateToken(loginRequest.Email);
            return Ok(tokenResponse);
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            try
            {
                _authService.AddUser(request.Email, request.Username, request.Password, request.RoleId);
                return Ok(new RegisterResponse { Message = "User registered successfully"});
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration error");
                return StatusCode(500, "An error occurred during registration");
            }
        }
    }
}