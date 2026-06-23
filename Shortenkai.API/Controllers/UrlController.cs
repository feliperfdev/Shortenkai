using Microsoft.AspNetCore.Mvc;
using Shortenkai.Application.Services;
using Shortenkai.Domain.Models;

namespace Shortenkai.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}")]
    public class UrlController : ControllerBase
    {

        private readonly IShortenkaiService _service;

        public UrlController(IShortenkaiService service)
        {
            _service = service;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAll();

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

        [HttpGet("get/{code}")]
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

        [HttpGet("{code}")]
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
