using ShortenURL.Models;
using System.Text;

namespace ShortenURL.Services
{
    public class UrlShorteningService
    {
        private readonly IConfiguration _configuration;
        private readonly List<UrlMapping> _urlMappings; // In-memory storage for demo
        private readonly Random _random;

        public UrlShorteningService(IConfiguration configuration)
        {
            _configuration = configuration;
            _urlMappings = new List<UrlMapping>();
            _random = new Random();
        }

        public async Task<ShortenResponse> ShortenUrlAsync(ShortenRequest request)
        {
            if (!IsValidUrl(request.OriginalUrl))
            {
                return new ShortenResponse
                {
                    Success = false,
                    Message = "Invalid URL format"
                };
            }

            // Check if custom alias already exists
            if (!string.IsNullOrEmpty(request.CustomAlias))
            {
                if (_urlMappings.Any(u => u.CustomAlias == request.CustomAlias || u.ShortCode == request.CustomAlias))
                {
                    return new ShortenResponse
                    {
                        Success = false,
                        Message = "Custom alias already exists"
                    };
                }
            }

            // Check if URL already exists for this user
            var existingMapping = _urlMappings.FirstOrDefault(u => 
                u.OriginalUrl == request.OriginalUrl && 
                u.UserId == request.UserId && 
                u.IsActive);

            if (existingMapping != null)
            {
                var baseUrl = _configuration["BaseUrl"] ?? "https://localhost:7098";
                return new ShortenResponse
                {
                    Success = true,
                    Message = "URL already shortened",
                    ShortUrl = $"{baseUrl}/{existingMapping.ShortCode}",
                    ShortCode = existingMapping.ShortCode,
                    OriginalUrl = existingMapping.OriginalUrl,
                    ExpiresAt = existingMapping.ExpiresAt
                };
            }

            var shortCode = !string.IsNullOrEmpty(request.CustomAlias) 
                ? request.CustomAlias 
                : GenerateShortCode();

            var mapping = new UrlMapping
            {
                Id = _urlMappings.Count + 1,
                OriginalUrl = request.OriginalUrl,
                ShortCode = shortCode,
                UserId = request.UserId,
                ExpiresAt = request.ExpiresAt,
                CustomAlias = request.CustomAlias
            };

            _urlMappings.Add(mapping);

            var shortUrl = $"{_configuration["BaseUrl"] ?? "https://localhost:7098"}/{shortCode}";

            return new ShortenResponse
            {
                Success = true,
                Message = "URL shortened successfully",
                ShortUrl = shortUrl,
                ShortCode = shortCode,
                OriginalUrl = request.OriginalUrl,
                ExpiresAt = request.ExpiresAt
            };
        }

        public async Task<string?> GetOriginalUrlAsync(string shortCode)
        {
            var mapping = _urlMappings.FirstOrDefault(u => 
                (u.ShortCode == shortCode || u.CustomAlias == shortCode) && 
                u.IsActive);

            if (mapping == null)
                return null;

            // Check if expired
            if (mapping.ExpiresAt.HasValue && mapping.ExpiresAt.Value < DateTime.UtcNow)
            {
                mapping.IsActive = false;
                return null;
            }

            // Increment click count
            mapping.ClickCount++;

            return mapping.OriginalUrl;
        }

        public async Task<UrlStats?> GetUrlStatsAsync(string shortCode)
        {
            var mapping = _urlMappings.FirstOrDefault(u => 
                (u.ShortCode == shortCode || u.CustomAlias == shortCode));

            if (mapping == null)
                return null;

            return new UrlStats
            {
                ShortCode = mapping.ShortCode,
                OriginalUrl = mapping.OriginalUrl,
                ClickCount = mapping.ClickCount,
                CreatedAt = mapping.CreatedAt,
                ExpiresAt = mapping.ExpiresAt,
                IsActive = mapping.IsActive
            };
        }

        public async Task<List<UrlStats>> GetUserUrlsAsync(int userId)
        {
            return _urlMappings
                .Where(u => u.UserId == userId)
                .Select(u => new UrlStats
                {
                    ShortCode = u.ShortCode,
                    OriginalUrl = u.OriginalUrl,
                    ClickCount = u.ClickCount,
                    CreatedAt = u.CreatedAt,
                    ExpiresAt = u.ExpiresAt,
                    IsActive = u.IsActive
                })
                .OrderByDescending(u => u.CreatedAt)
                .ToList();
        }

        public async Task<bool> DeleteUrlAsync(string shortCode, int? userId = null)
        {
            var mapping = _urlMappings.FirstOrDefault(u => 
                (u.ShortCode == shortCode || u.CustomAlias == shortCode) &&
                (userId == null || u.UserId == userId));

            if (mapping == null)
                return false;

            mapping.IsActive = false;
            return true;
        }

        private string GenerateShortCode(int length = 6)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var result = new StringBuilder();
            
            for (int i = 0; i < length; i++)
            {
                result.Append(chars[_random.Next(chars.Length)]);
            }

            var code = result.ToString();
            
            // Ensure uniqueness
            if (_urlMappings.Any(u => u.ShortCode == code))
            {
                return GenerateShortCode(length);
            }

            return code;
        }

        private bool IsValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out var result) &&
                   (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
        }
    }
}
