using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace upsa_api.Controllers
{
    [Route("api/[controller]")]
    [Produces("application/json")]
    [ApiController]
    public class ThemisController : ControllerBase
    {
        private readonly Services.Interfaces.IThemisService _themisService;

        public ThemisController(
            Services.Interfaces.IThemisService themisService)
        {
            _themisService = themisService;
        }

        [HttpGet("process/{number}")]
        public async Task<IActionResult> GetProcess(
            [FromRoute] string number)
        {
            return Ok(await _themisService.GetProcess(number));
        }

        [HttpPut("process/add-foward/{number}")]
        public async Task<IActionResult> AddProcessFoward(
            [FromBody] Services.ThemisService.AndamentoProcessoInput andamento,
            [FromRoute] string number)
        {
            return Ok(await _themisService.AddProcessFoward(number, andamento));
        }

        [HttpPut("process")]
        public async Task<IActionResult> PostProcess(
            [FromBody] Services.ThemisService.Processo process)
        {
            return Ok(await _themisService.PostProcess(process));
        }
    }
}