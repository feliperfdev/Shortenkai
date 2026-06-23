namespace Shortenkai.Application.DTOs
{
    public class ShortenedUrlDto
    {
        public string ShortCode { get; set; }
        public string KeyCode { get; set; }
        public string? Slug { get; set; }
    }
}
