using ManagerURL.Models;

namespace ManagerURL.Services
{
    public class UrlManagementService
    {
        private readonly IConfiguration _configuration;
        private readonly List<UrlInfo> _urls; // In-memory storage for demo
        private readonly List<ClickRecord> _clickRecords;

        public UrlManagementService(IConfiguration configuration)
        {
            _configuration = configuration;
            _urls = new List<UrlInfo>();
            _clickRecords = new List<ClickRecord>();
            SeedData();
        }

        public async Task<List<UrlInfo>> GetAllUrlsAsync(int? userId = null, int page = 1, int pageSize = 10)
        {
            var query = _urls.AsQueryable();
            
            if (userId.HasValue)
            {
                query = query.Where(u => u.UserId == userId.Value);
            }

            return query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .OrderByDescending(u => u.CreatedAt)
                .ToList();
        }

        public async Task<UrlInfo?> GetUrlByShortCodeAsync(string shortCode)
        {
            var url = _urls.FirstOrDefault(u => u.ShortCode == shortCode);
            if (url != null)
            {
                url.ClickHistory = _clickRecords
                    .Where(c => c.ShortCode == shortCode)
                    .OrderByDescending(c => c.ClickedAt)
                    .Take(100) // Last 100 clicks
                    .ToList();
            }
            return url;
        }

        public async Task<UrlInfo?> UpdateUrlAsync(string shortCode, UpdateUrlRequest request, int? userId = null)
        {
            var url = _urls.FirstOrDefault(u => u.ShortCode == shortCode && 
                                               (userId == null || u.UserId == userId));
            
            if (url == null)
                return null;

            if (!string.IsNullOrEmpty(request.OriginalUrl))
            {
                if (!IsValidUrl(request.OriginalUrl))
                    return null;
                url.OriginalUrl = request.OriginalUrl;
            }

            if (request.ExpiresAt.HasValue)
            {
                url.ExpiresAt = request.ExpiresAt.Value;
            }

            if (request.IsActive.HasValue)
            {
                url.IsActive = request.IsActive.Value;
            }

            return url;
        }

        public async Task<bool> DeleteUrlAsync(string shortCode, int? userId = null)
        {
            var url = _urls.FirstOrDefault(u => u.ShortCode == shortCode && 
                                               (userId == null || u.UserId == userId));
            
            if (url == null)
                return false;

            _urls.Remove(url);
            
            // Also remove click records
            _clickRecords.RemoveAll(c => c.ShortCode == shortCode);
            
            return true;
        }

        public async Task<UrlAnalytics?> GetUrlAnalyticsAsync(string shortCode, int? userId = null)
        {
            var url = _urls.FirstOrDefault(u => u.ShortCode == shortCode && 
                                               (userId == null || u.UserId == userId));
            
            if (url == null)
                return null;

            var clicks = _clickRecords.Where(c => c.ShortCode == shortCode).ToList();
            
            var dailyClicks = clicks
                .GroupBy(c => c.ClickedAt.Date)
                .Select(g => new DailyClick { Date = g.Key, Count = g.Count() })
                .OrderBy(d => d.Date)
                .ToList();

            var countryClicks = clicks
                .Where(c => !string.IsNullOrEmpty(c.Country))
                .GroupBy(c => c.Country)
                .Select(g => new CountryClick { Country = g.Key!, Count = g.Count() })
                .OrderByDescending(c => c.Count)
                .ToList();

            var referrerClicks = clicks
                .Where(c => !string.IsNullOrEmpty(c.Referrer))
                .GroupBy(c => c.Referrer)
                .Select(g => new ReferrerClick { Referrer = g.Key!, Count = g.Count() })
                .OrderByDescending(r => r.Count)
                .ToList();

            return new UrlAnalytics
            {
                ShortCode = url.ShortCode,
                OriginalUrl = url.OriginalUrl,
                TotalClicks = url.ClickCount,
                CreatedAt = url.CreatedAt,
                LastClickAt = clicks.OrderByDescending(c => c.ClickedAt).FirstOrDefault()?.ClickedAt,
                DailyClicks = dailyClicks,
                CountryClicks = countryClicks,
                ReferrerClicks = referrerClicks
            };
        }

        public async Task<BulkOperationResponse> BulkOperationAsync(BulkOperationRequest request, int? userId = null)
        {
            var response = new BulkOperationResponse
            {
                Success = true,
                Message = "Bulk operation completed"
            };

            foreach (var shortCode in request.ShortCodes)
            {
                var url = _urls.FirstOrDefault(u => u.ShortCode == shortCode && 
                                                   (userId == null || u.UserId == userId));
                
                if (url == null)
                {
                    response.FailureCount++;
                    response.FailedItems.Add(shortCode);
                    continue;
                }

                switch (request.Operation.ToLower())
                {
                    case "delete":
                        _urls.Remove(url);
                        _clickRecords.RemoveAll(c => c.ShortCode == shortCode);
                        break;
                    case "activate":
                        url.IsActive = true;
                        break;
                    case "deactivate":
                        url.IsActive = false;
                        break;
                    default:
                        response.FailureCount++;
                        response.FailedItems.Add(shortCode);
                        continue;
                }

                response.SuccessCount++;
            }

            if (response.FailureCount > 0)
            {
                response.Success = false;
                response.Message = $"Bulk operation completed with {response.FailureCount} failures";
            }

            return response;
        }

        public async Task RecordClickAsync(string shortCode, string ipAddress, string userAgent, string? referrer = null)
        {
            var url = _urls.FirstOrDefault(u => u.ShortCode == shortCode);
            if (url != null)
            {
                url.ClickCount++;
                
                var clickRecord = new ClickRecord
                {
                    Id = _clickRecords.Count + 1,
                    ShortCode = shortCode,
                    ClickedAt = DateTime.UtcNow,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    Referrer = referrer,
                    Country = GetCountryFromIP(ipAddress), // Simplified
                    City = GetCityFromIP(ipAddress) // Simplified
                };

                _clickRecords.Add(clickRecord);
            }
        }

        public async Task<Dictionary<string, object>> GetDashboardStatsAsync(int? userId = null)
        {
            var query = _urls.AsQueryable();
            if (userId.HasValue)
            {
                query = query.Where(u => u.UserId == userId.Value);
            }

            var urls = query.ToList();
            var totalClicks = urls.Sum(u => u.ClickCount);
            var activeUrls = urls.Count(u => u.IsActive);
            var expiredUrls = urls.Count(u => u.ExpiresAt.HasValue && u.ExpiresAt.Value < DateTime.UtcNow);

            return new Dictionary<string, object>
            {
                ["totalUrls"] = urls.Count,
                ["activeUrls"] = activeUrls,
                ["expiredUrls"] = expiredUrls,
                ["totalClicks"] = totalClicks,
                ["averageClicksPerUrl"] = urls.Count > 0 ? totalClicks / (double)urls.Count : 0,
                ["topUrls"] = urls.OrderByDescending(u => u.ClickCount).Take(5).ToList()
            };
        }

        private void SeedData()
        {
            // Add some sample data
            _urls.Add(new UrlInfo
            {
                Id = 1,
                OriginalUrl = "https://www.google.com",
                ShortCode = "google1",
                ShortUrl = "https://localhost:7002/google1",
                UserId = 1,
                UserName = "admin",
                CreatedAt = DateTime.UtcNow.AddDays(-7),
                ClickCount = 25,
                IsActive = true
            });

            _urls.Add(new UrlInfo
            {
                Id = 2,
                OriginalUrl = "https://github.com",
                ShortCode = "github1",
                ShortUrl = "https://localhost:7002/github1",
                UserId = 1,
                UserName = "admin",
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                ClickCount = 12,
                IsActive = true
            });
        }

        private bool IsValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out var result) &&
                   (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
        }

        private string GetCountryFromIP(string ipAddress)
        {
            // Simplified - in real implementation, use GeoIP service
            return "Vietnam";
        }

        private string GetCityFromIP(string ipAddress)
        {
            // Simplified - in real implementation, use GeoIP service
            return "Ho Chi Minh City";
        }
    }
}
