using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using upsa_api.Models;

namespace upsa_api.Controllers
{
    [Route("api/[controller]")]
    [Produces("application/json")]
    [ApiController]
    public class MessageController : Controller
    {
        private readonly Services.Interfaces.IMessageService messageService;

        public MessageController(
            Services.Interfaces.IMessageService _messageService)
        {
            messageService = _messageService;
        }

        [HttpPost("notify-avocado")]
        public async Task<IActionResult> SendNotifyAvocados(
            [FromBody] NotifyAvocadoModel notify)
        {
            return Ok(await messageService.SendNotifyToAvocados(notify));
        }
    }
}
