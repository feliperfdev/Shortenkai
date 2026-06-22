using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
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
        private readonly CacheService _cacheService;

        public ShortenkaiService(ShortenkaiUrlDb context, CacheService cacheService)
        {
            _context = context;
            _cacheService = cacheService;
        }

        public async Task<FAResult<List<ShortenedUrlDto>>> GetAll()
        {
            var result = await _context.Urls.ToListAsync();

            if (result == null)
            {
                return FAResult<List<ShortenedUrlDto>>.Failure("It wasn't possible to get all shortned urls!");
            }

            var list = result.Select(u => new ShortenedUrlDto { ShortCode = u.ShortCode, KeyCode = u.KeyCode, Slug = u.Slug }).ToList();

            return FAResult<List<ShortenedUrlDto>>.Success(list);
        }

        public async Task<FAResult<ShortenedUrlDto>> GetByCode(string code)
        {
            var result = _context.Urls.SingleOrDefault(shortUrl => shortUrl.ShortCode == code);

            if (result == null)
            {
                return FAResult<ShortenedUrlDto>.Failure("It wasn't possible to get the shortned url with the parsed code!");
            }

            return FAResult<ShortenedUrlDto>.Success(new ShortenedUrlDto { KeyCode = result.KeyCode, ShortCode = result.ShortCode, Slug = result.Slug });
        }

        public async Task<FAResult<string>> GetUrlByCode(string code)
        {
            var fromCache = await _cacheService.GetAsync<string>(code);

            if (fromCache != null) {
                return FAResult<string>.Success(fromCache);
            }

            var result = _context.Urls.SingleOrDefault(shortUrl => shortUrl.ShortCode == code || shortUrl.Slug == code);

            if (result == null)
            {
                return FAResult<string>.Failure("It wasn't possible to redirect to url with the parsed code or slug!");
            }

            return FAResult<string>.Success(result.OriginalUrl);
        }

        public async Task<FAResult<ShortenedUrlDto>> ShortCodes(string url, string? slug)
        {
            try
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(url);
                byte[] hashBytes = SHA256.HashData(inputBytes);
                string hashHex = Convert.ToHexString(hashBytes).ToLower();

                string shortCode = hashHex.Substring(0, 10);
                string keyCode = hashHex.Substring(9);

                var fromCache = await _cacheService.GetAsync<string>(shortCode);

                if (fromCache != null) { return FAResult<ShortenedUrlDto>.Failure("This same shortened URL already exists."); }

                ShortenkaiUrl shortenkai = new ShortenkaiUrl { OriginalUrl = url, ShortCode = shortCode, KeyCode = keyCode, Slug = slug };

                _context.Urls.Add(shortenkai);
                await _context.SaveChangesAsync();

                await _cacheService.SetAsync(slug ?? shortCode, url, TimeSpan.FromDays(30));

                return FAResult<ShortenedUrlDto>.Success(new ShortenedUrlDto { KeyCode = keyCode, ShortCode = shortCode, Slug = slug });
            } catch (Exception ex)
            {
                return FAResult<ShortenedUrlDto>.Failure(ex.Message);
            }
        }
    }
}
