using Imel.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Imel.API.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class AuditLogsController : Controller
    {
        private readonly ILogger<AuditLogsController> _logger;

        public AuditLogsController(ILogger<AuditLogsController> logger)
        {
            _logger = logger;
        }

        [HttpGet()]
        [Authorize(Roles = "Admin")]
        public IActionResult GetAuditLogs()
        {
            return Ok(DBContext.AuditLogs.ToList().OrderByDescending(x => x.Id));
        }
    }
}