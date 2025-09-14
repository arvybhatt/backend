using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServiceBusSenderController : ControllerBase
    {
        private readonly ServiceBusSenderService _senderService;

        public ServiceBusSenderController(ServiceBusSenderService senderService)
        {
            _senderService = senderService;
        }

        /// <summary>
        /// Sends a test message to the Azure Service Bus queue
        /// </summary>
        /// <param name="message">Message content to send</param>
        /// <returns>Success confirmation with the sent message</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> SendTestMessage([FromBody] string message)
        {
            await _senderService.SendTestMessageAsync(message);
            return Ok(new { status = "Message sent", message });
        }
    }
}
