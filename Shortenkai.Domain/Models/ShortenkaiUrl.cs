using System.ComponentModel.DataAnnotations;

namespace Shortenkai.Domain.Models
{
    public class ShortenkaiUrl
    {
        public string OriginalUrl { get; set; }
        
        [Key]
        public string ShortCode { get; set; }
        public string KeyCode { get; set; }
        public string? Slug { get; set; }
    }
}
