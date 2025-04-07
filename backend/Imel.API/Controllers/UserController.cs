using Imel.Interfaces;
using Imel.Models.User;
using Microsoft.AspNetCore.Mvc;

namespace Imel.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserCRUD _userCRUD;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserCRUD userCRUD, ILogger<UsersController> logger)
        {
            _userCRUD = userCRUD;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult GetAllUsers()
        {
            var users = _userCRUD.GetAllUsers();
            return Ok(users);
        }

        [HttpGet("{email}")]
        public IActionResult GetUser(string email)
        {
            try
            {
                var user = _userCRUD.GetUserByEmail(email);
                return Ok(user);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPut("{email}")]
        public IActionResult UpdateUser(string email, [FromBody] UpdateUserRequest request)
        {
            try
            {
                var user = _userCRUD.UpdateUser(
                    email,
                    request.Username,
                    request.Password,
                    request.Role,
                    request.IsActive);

                return Ok(user);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{email}")]
        public IActionResult DeleteUser(string email)
        {
            try
            {
                var result = _userCRUD.DeleteUser(email);
                return result ? NoContent() : NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("{email}/status")]
        public IActionResult ToggleUserStatus(string email)
        {
            try
            {
                var isActive = _userCRUD.ToggleUserStatus(email);
                return Ok(new { Email = email, IsActive = isActive });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}