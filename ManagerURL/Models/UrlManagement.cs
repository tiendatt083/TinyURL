namespace ManagerURL.Models
{
    public class UrlInfo
    {
        public int Id { get; set; }
        public string OriginalUrl { get; set; } = string.Empty;
        public string ShortCode { get; set; } = string.Empty;
        public string ShortUrl { get; set; } = string.Empty;
        public int? UserId { get; set; }
        public string? UserName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public int ClickCount { get; set; }
        public bool IsActive { get; set; }
        public string? CustomAlias { get; set; }
        public List<ClickRecord> ClickHistory { get; set; } = new List<ClickRecord>();
    }

    public class ClickRecord
    {
        public int Id { get; set; }
        public string ShortCode { get; set; } = string.Empty;
        public DateTime ClickedAt { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
        public string? Referrer { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }
    }

    public class UpdateUrlRequest
    {
        public string? OriginalUrl { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public bool? IsActive { get; set; }
    }

    public class UrlAnalytics
    {
        public string ShortCode { get; set; } = string.Empty;
        public string OriginalUrl { get; set; } = string.Empty;
        public int TotalClicks { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastClickAt { get; set; }
        public List<DailyClick> DailyClicks { get; set; } = new List<DailyClick>();
        public List<CountryClick> CountryClicks { get; set; } = new List<CountryClick>();
        public List<ReferrerClick> ReferrerClicks { get; set; } = new List<ReferrerClick>();
    }

    public class DailyClick
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
    }

    public class CountryClick
    {
        public string Country { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class ReferrerClick
    {
        public string Referrer { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class BulkOperationRequest
    {
        public List<string> ShortCodes { get; set; } = new List<string>();
        public string Operation { get; set; } = string.Empty; // "delete", "activate", "deactivate"
    }

    public class BulkOperationResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public List<string> FailedItems { get; set; } = new List<string>();
    }
}
