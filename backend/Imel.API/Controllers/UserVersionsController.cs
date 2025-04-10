using Imel.Interfaces;
using Imel.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Imel.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserVersionsController : ControllerBase
    {
        private readonly IUserVersionsService _userVersionsService;
        private readonly ILogger<UserVersionsController> _logger;

        public UserVersionsController(IUserVersionsService userVersionsService, ILogger<UserVersionsController> logger)
        {
            _userVersionsService = userVersionsService;
            _logger = logger;
        }

        [HttpGet("{userId}")]
        public IActionResult GetUser(int userId)
        {
            try
            {
                var versions = _userVersionsService.GetUserVersions(userId);
                return Ok(versions);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}