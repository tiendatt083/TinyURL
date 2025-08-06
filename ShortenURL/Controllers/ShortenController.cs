using Microsoft.AspNetCore.Mvc;
using ShortenURL.Models;
using ShortenURL.Services;

namespace ShortenURL.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShortenController : ControllerBase
    {
        private readonly UrlShorteningService _urlService;

        public ShortenController(UrlShorteningService urlService)
        {
            _urlService = urlService;
        }

        /// <summary>
        /// Rút gọn URL
        /// </summary>
        /// <param name="request">Thông tin URL cần rút gọn</param>
        /// <returns>URL đã được rút gọn</returns>
        [HttpPost]
        public async Task<ActionResult<ShortenResponse>> ShortenUrl([FromBody] ShortenRequest request)
        {
            if (string.IsNullOrEmpty(request.OriginalUrl))
            {
                return BadRequest(new ShortenResponse
                {
                    Success = false,
                    Message = "Original URL is required"
                });
            }

            var result = await _urlService.ShortenUrlAsync(request);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Redirect đến URL gốc
        /// </summary>
        /// <param name="shortCode">Mã rút gọn</param>
        /// <returns>Redirect đến URL gốc</returns>
        [HttpGet("{shortCode}")]
        public async Task<IActionResult> RedirectToOriginal(string shortCode)
        {
            var originalUrl = await _urlService.GetOriginalUrlAsync(shortCode);
            
            if (string.IsNullOrEmpty(originalUrl))
            {
                return NotFound(new { message = "Short URL not found or expired" });
            }

            return Redirect(originalUrl);
        }

        /// <summary>
        /// Lấy thống kê URL
        /// </summary>
        /// <param name="shortCode">Mã rút gọn</param>
        /// <returns>Thống kê URL</returns>
        [HttpGet("{shortCode}/stats")]
        public async Task<ActionResult<UrlStats>> GetUrlStats(string shortCode)
        {
            var stats = await _urlService.GetUrlStatsAsync(shortCode);
            
            if (stats == null)
            {
                return NotFound(new { message = "Short URL not found" });
            }

            return Ok(stats);
        }

        /// <summary>
        /// Lấy danh sách URL của user
        /// </summary>
        /// <param name="userId">ID của user</param>
        /// <returns>Danh sách URL</returns>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<UrlStats>>> GetUserUrls(int userId)
        {
            var urls = await _urlService.GetUserUrlsAsync(userId);
            return Ok(urls);
        }

        /// <summary>
        /// Xóa URL rút gọn
        /// </summary>
        /// <param name="shortCode">Mã rút gọn</param>
        /// <param name="userId">ID của user (optional)</param>
        /// <returns>Kết quả xóa</returns>
        [HttpDelete("{shortCode}")]
        public async Task<IActionResult> DeleteUrl(string shortCode, [FromQuery] int? userId = null)
        {
            var success = await _urlService.DeleteUrlAsync(shortCode, userId);
            
            if (!success)
            {
                return NotFound(new { message = "Short URL not found" });
            }

            return Ok(new { message = "URL deleted successfully" });
        }

        /// <summary>
        /// Kiểm tra tính khả dụng của custom alias
        /// </summary>
        /// <param name="alias">Custom alias cần kiểm tra</param>
        /// <returns>Tình trạng khả dụng</returns>
        [HttpGet("check-alias/{alias}")]
        public async Task<ActionResult> CheckAliasAvailability(string alias)
        {
            // This is a simple check - in real implementation, you'd check against the database
            var available = !string.IsNullOrEmpty(alias) && alias.Length >= 3;
            
            return Ok(new { 
                alias = alias,
                available = available,
                message = available ? "Alias is available" : "Alias is not available or invalid"
            });
        }
    }
}
