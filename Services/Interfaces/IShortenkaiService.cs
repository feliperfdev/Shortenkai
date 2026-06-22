using Shortenkai.DTOs;
using Shortenkai.Utils;

namespace Shortenkai.Services.Interfaces
{
    public interface IShortenkaiService
    {
        Task<FAResult<ShortenedUrlDto>> GetByCode(string code);
        Task<FAResult<string>> GetUrlByCode(string code);

        Task<FAResult<ShortenedUrlDto>> ShortCodes(string url, string? slug);
    }
}
