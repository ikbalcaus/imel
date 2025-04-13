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

        [HttpPost("generete-test-users")]
        public ActionResult GenerateUsers()
        {
            int id = DBContext.Users.Any() ? DBContext.Users.Max(x => x.Id) + 1 : 1;
            var testUsers = new List<object>();

            string[] emailList = new string[] { "a@a.a", "e@e.e", "i@i.i", "o@o.o", "u@u.u" };
            string password = "12345678";

            foreach (var email in emailList)
            {
                if (!DBContext.Users.Any(x => x.Email == email))
                {
                    var user = new User
                    {
                        Id = id++,
                        Email = email,
                        Username = email.Split('@')[0],
                        PasswordHash = Helpers.HashPassword(password),
                        RoleId = 2,
                        Role = DBContext.Roles.FirstOrDefault(x => (x.Id == 2))!
                    };
                    DBContext.Users.Add(user);
                    Helpers.CreateUserVersion(user, "CREATE");
                    Helpers.CreateAuditLog("User", user.Id, "CREATE", "", user);
                    testUsers.Add(new { email = email, password = password });
                }
            }
            return Ok(testUsers);
        }

        [HttpGet("verify-admin")]
        [Authorize(Roles = "Admin")]
        public IActionResult VerifyAdmin()
        {
            return NoContent();
        }
    }
}