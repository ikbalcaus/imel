using Imel.Interfaces;
using Imel.Models.UserVersions;
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
        [Authorize(Roles = "Admin")]
        public IActionResult GetUserVersion(int userId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var versions = _userVersionsService.GetUsersVersions(userId, pageNumber, pageSize);
                return Ok(versions);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPut("{userId}")]
        [Authorize(Roles = "Admin")]
        public IActionResult RevertUserVersion(int userId, [FromBody] RevertUserVersionRequest req)
        {
            try
            {
                var user = _userVersionsService.RevertUserVersion(userId, req);
                return Ok(user);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}