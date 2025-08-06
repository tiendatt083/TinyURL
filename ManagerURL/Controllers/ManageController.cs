using Microsoft.AspNetCore.Mvc;
using ManagerURL.Models;
using ManagerURL.Services;

namespace ManagerURL.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ManageController : ControllerBase
    {
        private readonly UrlManagementService _managementService;

        public ManageController(UrlManagementService managementService)
        {
            _managementService = managementService;
        }

        /// <summary>
        /// Lấy danh sách tất cả URL
        /// </summary>
        /// <param name="userId">ID của user (optional)</param>
        /// <param name="page">Trang hiện tại</param>
        /// <param name="pageSize">Số item trên mỗi trang</param>
        /// <returns>Danh sách URL</returns>
        [HttpGet("urls")]
        public async Task<ActionResult<List<UrlInfo>>> GetAllUrls(
            [FromQuery] int? userId = null, 
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 10)
        {
            var urls = await _managementService.GetAllUrlsAsync(userId, page, pageSize);
            return Ok(urls);
        }

        /// <summary>
        /// Lấy thông tin chi tiết URL theo short code
        /// </summary>
        /// <param name="shortCode">Mã rút gọn</param>
        /// <returns>Thông tin chi tiết URL</returns>
        [HttpGet("urls/{shortCode}")]
        public async Task<ActionResult<UrlInfo>> GetUrlDetails(string shortCode)
        {
            var url = await _managementService.GetUrlByShortCodeAsync(shortCode);
            
            if (url == null)
            {
                return NotFound(new { message = "URL not found" });
            }

            return Ok(url);
        }

        /// <summary>
        /// Cập nhật thông tin URL
        /// </summary>
        /// <param name="shortCode">Mã rút gọn</param>
        /// <param name="request">Thông tin cập nhật</param>
        /// <param name="userId">ID của user (optional)</param>
        /// <returns>Thông tin URL đã cập nhật</returns>
        [HttpPut("urls/{shortCode}")]
        public async Task<ActionResult<UrlInfo>> UpdateUrl(
            string shortCode, 
            [FromBody] UpdateUrlRequest request,
            [FromQuery] int? userId = null)
        {
            var url = await _managementService.UpdateUrlAsync(shortCode, request, userId);
            
            if (url == null)
            {
                return NotFound(new { message = "URL not found or invalid data" });
            }

            return Ok(url);
        }

        /// <summary>
        /// Xóa URL
        /// </summary>
        /// <param name="shortCode">Mã rút gọn</param>
        /// <param name="userId">ID của user (optional)</param>
        /// <returns>Kết quả xóa</returns>
        [HttpDelete("urls/{shortCode}")]
        public async Task<IActionResult> DeleteUrl(string shortCode, [FromQuery] int? userId = null)
        {
            var success = await _managementService.DeleteUrlAsync(shortCode, userId);
            
            if (!success)
            {
                return NotFound(new { message = "URL not found" });
            }

            return Ok(new { message = "URL deleted successfully" });
        }

        /// <summary>
        /// Lấy thống kê chi tiết của URL
        /// </summary>
        /// <param name="shortCode">Mã rút gọn</param>
        /// <param name="userId">ID của user (optional)</param>
        /// <returns>Thống kê chi tiết</returns>
        [HttpGet("urls/{shortCode}/analytics")]
        public async Task<ActionResult<UrlAnalytics>> GetUrlAnalytics(
            string shortCode, 
            [FromQuery] int? userId = null)
        {
            var analytics = await _managementService.GetUrlAnalyticsAsync(shortCode, userId);
            
            if (analytics == null)
            {
                return NotFound(new { message = "URL not found" });
            }

            return Ok(analytics);
        }

        /// <summary>
        /// Thực hiện thao tác hàng loạt
        /// </summary>
        /// <param name="request">Thông tin thao tác hàng loạt</param>
        /// <param name="userId">ID của user (optional)</param>
        /// <returns>Kết quả thao tác</returns>
        [HttpPost("bulk")]
        public async Task<ActionResult<BulkOperationResponse>> BulkOperation(
            [FromBody] BulkOperationRequest request,
            [FromQuery] int? userId = null)
        {
            if (request.ShortCodes == null || !request.ShortCodes.Any())
            {
                return BadRequest(new { message = "Short codes are required" });
            }

            if (string.IsNullOrEmpty(request.Operation))
            {
                return BadRequest(new { message = "Operation is required" });
            }

            var result = await _managementService.BulkOperationAsync(request, userId);
            return Ok(result);
        }

        /// <summary>
        /// Ghi nhận click
        /// </summary>
        /// <param name="shortCode">Mã rút gọn</param>
        /// <returns>Kết quả ghi nhận</returns>
        [HttpPost("urls/{shortCode}/click")]
        public async Task<IActionResult> RecordClick(string shortCode)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var userAgent = Request.Headers["User-Agent"].FirstOrDefault() ?? "unknown";
            var referrer = Request.Headers["Referer"].FirstOrDefault();

            await _managementService.RecordClickAsync(shortCode, ipAddress, userAgent, referrer);
            
            return Ok(new { message = "Click recorded successfully" });
        }

        /// <summary>
        /// Lấy thống kê tổng quan
        /// </summary>
        /// <param name="userId">ID của user (optional)</param>
        /// <returns>Thống kê tổng quan</returns>
        [HttpGet("dashboard")]
        public async Task<ActionResult<Dictionary<string, object>>> GetDashboardStats(
            [FromQuery] int? userId = null)
        {
            var stats = await _managementService.GetDashboardStatsAsync(userId);
            return Ok(stats);
        }

        /// <summary>
        /// Export danh sách URL ra CSV
        /// </summary>
        /// <param name="userId">ID của user (optional)</param>
        /// <returns>File CSV</returns>
        [HttpGet("export/csv")]
        public async Task<IActionResult> ExportToCsv([FromQuery] int? userId = null)
        {
            var urls = await _managementService.GetAllUrlsAsync(userId, 1, int.MaxValue);
            
            var csvContent = "ShortCode,OriginalUrl,CreatedAt,ClickCount,IsActive,ExpiresAt\n";
            foreach (var url in urls)
            {
                csvContent += $"{url.ShortCode},{url.OriginalUrl},{url.CreatedAt:yyyy-MM-dd},{url.ClickCount},{url.IsActive},{url.ExpiresAt?.ToString("yyyy-MM-dd") ?? ""}\n";
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(csvContent);
            return File(bytes, "text/csv", $"urls_export_{DateTime.UtcNow:yyyyMMdd}.csv");
        }
    }
}
