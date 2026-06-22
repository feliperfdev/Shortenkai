using Microsoft.AspNetCore.Mvc;
using Shortenkai.Services.Interfaces;

namespace Shortenkai.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class ShortenkaiController : ControllerBase
    {

        private readonly IShortenkaiService _service;

        public ShortenkaiController(IShortenkaiService service)
        {
            _service = service;
        }

        [HttpGet("{code}")]
        public async Task<IActionResult> GetByCode(string code)
        {
            var result = await _service.GetByCode(code);

            if (result.IsNotFound)
            {
                return NotFound();
            }

            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return BadRequest();
        }

        [HttpGet("/url/{code}")]
        public async Task<IActionResult> RedirectUrl(string code)
        {
            var result = await _service.GetUrlByCode(code);

            if (result.IsNotFound)
            {
                return NotFound();
            }

            if (result.IsSuccess)
            {
                return Redirect(result.Value!);
            }

            return BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> ShortCodes(
            [FromBody] RequestShortenkai req)
        {
            var result = await _service.ShortCodes(req.Url, req.Slug);

            if (result.IsNotFound)
            {
                return NotFound();
            }

            if (result.IsSuccess)
            {
                return CreatedAtAction(nameof(GetByCode), new { code = result.Value!.KeyCode }, result.Value!);
            }

            return BadRequest();
        }
    }
}
