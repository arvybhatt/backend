using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using Backend.Services;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServiceBusMessagesController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetMessages()
        {
            return Ok(ServiceBusMessageStore.Messages.ToArray());
        }
    }
}
