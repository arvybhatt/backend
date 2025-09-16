using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Backend.Data;
using System;
using System.Threading.Tasks;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly TodoDbContext _context;
        private readonly ILogger<HealthController> _logger;

        public HealthController(TodoDbContext context, ILogger<HealthController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/health
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            _logger?.LogInformation("GET api/health called");

            try
            {
                var canConnect = await _context.Database.CanConnectAsync();

                if (canConnect)
                {
                    _logger?.LogInformation("Health check OK - database reachable");
                    return Ok(new { status = "Healthy", database = "Connected" });
                }

                _logger?.LogWarning("Health check degraded - database unreachable");
                return StatusCode(503, new { status = "Degraded", database = "Unavailable" });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Health check failed with exception");
                return StatusCode(503, new { status = "Unhealthy", error = ex.Message });
            }
        }
    }
}
