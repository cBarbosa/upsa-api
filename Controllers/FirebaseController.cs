using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using upsa_api.Services;

namespace upsa_api.Controllers
{
    [Route("api/[controller]")]
    [Produces("application/json")]
    [ApiController]
    public class FirebaseController : ControllerBase
    {
        private readonly FirebaseService _firebaseService;
        private Services.Interfaces.IMessageService _message;

        public FirebaseController(
            FirebaseService firebaseService,
            Services.Interfaces.IMessageService message
            )
        {
            _firebaseService = firebaseService;
            _message = message;
        }

        [HttpGet("process/send-mail")]
        public async Task<IActionResult> SendMail()
        {
            return Ok(await _message.SendDaylyNotification());
        }
    }
}