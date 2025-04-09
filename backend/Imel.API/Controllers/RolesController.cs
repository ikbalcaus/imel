using Imel.Interfaces;
using Imel.Models.User;
using Imel.Services;
using Microsoft.AspNetCore.Mvc;

namespace Imel.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly IRolesService _rolesService;
        private readonly ILogger<RolesController> _logger;

        public RolesController(IRolesService rolesService, ILogger<RolesController> logger)
        {
            _rolesService = rolesService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult GetAllUsers()
        {
            var users = _rolesService.GetAllRoles();
            return Ok(users);
        }
    }
}