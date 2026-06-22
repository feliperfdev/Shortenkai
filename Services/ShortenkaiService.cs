using Microsoft.AspNetCore.Http.HttpResults;
using Shortenkai.Database;
using Shortenkai.DTOs;
using Shortenkai.Models;
using Shortenkai.Services.Interfaces;
using Shortenkai.Utils;
using System.Security.Cryptography;
using System.Text;

namespace Shortenkai.Services
{
    public class ShortenkaiService : IShortenkaiService
    {
        private readonly ShortenkaiUrlDb _context;

        public ShortenkaiService(ShortenkaiUrlDb context)
        {
            _context = context;
        }

        public async Task<FAResult<ShortenedUrlDto>> GetByCode(string code)
        {
            var result = _context.Urls.SingleOrDefault(shortUrl => shortUrl.ShortCode == code);

            if (result == null)
            {
                return FAResult<ShortenedUrlDto>.Failure("");
            }

            return FAResult<ShortenedUrlDto>.Success(new ShortenedUrlDto { KeyCode = result.KeyCode, ShortCode = result.ShortCode, Slug = result.Slug });
        }

        public async Task<FAResult<string>> GetUrlByCode(string code)
        {
            var result = _context.Urls.SingleOrDefault(shortUrl => shortUrl.ShortCode == code);

            if (result == null)
            {
                return FAResult<string>.Failure("");
            }

            return FAResult<string>.Success(result.OriginalUrl);
        }

        public async Task<FAResult<ShortenedUrlDto>> ShortCodes(string url, string? slug)
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(url);
            byte[] hashBytes = SHA256.HashData(inputBytes);
            string hashHex = Convert.ToHexString(hashBytes).ToLower();

            string shortCode = hashHex.Substring(0, 10);
            string keyCode = hashHex.Substring(9);

            ShortenkaiUrl shortenkai = new ShortenkaiUrl { OriginalUrl = url, ShortCode = shortCode, KeyCode = keyCode, Slug = slug };

            _context.Urls.Add(shortenkai);
            await _context.SaveChangesAsync();

            return FAResult<ShortenedUrlDto>.Success(new ShortenedUrlDto { KeyCode = keyCode, ShortCode = shortCode, Slug = slug });
        }
    }
}
