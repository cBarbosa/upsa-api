using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace upsa_api.Controllers
{
    [Route("api/[controller]")]
    [Produces("application/json")]
    [ApiController]
    public class FirebaseController : ControllerBase
    {
        private Services.Interfaces.IMessageService _message;

        public FirebaseController(Services.Interfaces.IMessageService message)
        {
            _message = message;
        }

        [HttpGet("process/send-mail")]
        public async Task<IActionResult> SendMail()
        {
            return Ok(await _message.SendDaylyNotification());
        }
    }
}