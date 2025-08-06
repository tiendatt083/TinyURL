namespace ShortenURL.Models
{
    public class UrlMapping
    {
        public int Id { get; set; }
        public string OriginalUrl { get; set; } = string.Empty;
        public string ShortCode { get; set; } = string.Empty;
        public int? UserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiresAt { get; set; }
        public int ClickCount { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public string? CustomAlias { get; set; }
    }

    public class ShortenRequest
    {
        public string OriginalUrl { get; set; } = string.Empty;
        public string? CustomAlias { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public int? UserId { get; set; }
    }

    public class ShortenResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? ShortUrl { get; set; }
        public string? ShortCode { get; set; }
        public string? OriginalUrl { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }

    public class UrlStats
    {
        public string ShortCode { get; set; } = string.Empty;
        public string OriginalUrl { get; set; } = string.Empty;
        public int ClickCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public bool IsActive { get; set; }
    }
}
