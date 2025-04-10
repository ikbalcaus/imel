using Imel.Database.Models;
using Imel.Database;
using Imel.Interfaces;
using Imel.Models;
using Imel.Models.Auth;
using Imel.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

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
        public IActionResult Login([FromBody] LoginRequest req)
        {
            var result = _authService.ValidateUser(req);

            if (!result.Success)
            {
                _logger.LogWarning($"Failed login attempt for {req.Email}. Remaining attempts: ");
                return Unauthorized(result);
            }

            var tokenResponse = _authService.GenerateToken(req);
            return Ok(tokenResponse);
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest req)
        {
            try
            {
                _authService.AddUser(req);
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

        [HttpPost("generete-admin")]
        public ActionResult GenereteAdminUser()
        {
            int id = DBContext.Users.Any() ? DBContext.Users.Max(x => x.Value.Id) + 1 : 1;
            if (!DBContext.Users.Values.Any(x => x.Email == "admin@admin.com"))
            {
                var user = new User
                {
                    Id = id,
                    Email = "admin@admin.com",
                    Username = "admin",
                    PasswordHash = Helpers.HashPassword("admin123"),
                    RoleId = 2,
                    Role = DBContext.Roles.FirstOrDefault(x => (x.Id == 2))!,
                    LastModified = DateTime.Now
                };
                DBContext.Users[id] = user;
                Helpers.CreateUserVersion(user, "CREATE");
            }
            return Ok(new RegisterResponse { Message = "email: admin@admin.com, password: admin123" });
        }

        [HttpGet("verify-admin")]
        [Authorize(Roles = "Admin")]
        public IActionResult VerifyAdmin()
        {
            return NoContent();
        }
    }
}