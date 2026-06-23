using Shortenkai.Application.Common;
using Shortenkai.Application.DTOs;

namespace Shortenkai.Application.Services
{
    public interface IShortenkaiService
    {
        Task<FAResult<List<ShortenedUrlDto>>> GetAll();
        Task<FAResult<ShortenedUrlDto>> GetByCode(string code);
        Task<FAResult<string>> GetUrlByCode(string code);

        Task<FAResult<ShortenedUrlDto>> ShortCodes(string url, string? slug);
    }
}
