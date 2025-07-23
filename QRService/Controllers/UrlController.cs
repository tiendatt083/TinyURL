using Microsoft.AspNetCore.Mvc;
using QrService.Services;

namespace QrService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UrlController : ControllerBase
    {
        private readonly QrCodeService _qrService;

        public UrlController(QrCodeService qrService)
        {
            _qrService = qrService;
        }

        /// <summary>
        /// Trả về ảnh QR code (dạng PNG) cho đường dẫn tương ứng với short code.
        /// </summary>
        /// <param name="code">Short URL code</param>
        /// <returns>Ảnh QR code PNG</returns>
        /// <response code="200">Thành công, trả về ảnh</response>
        /// <response code="400">Thiếu mã code</response>
        [HttpGet("{code}/qr")]
        [Produces("image/png")]
        public IActionResult GetQrImage(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return BadRequest("Code is required.");
            }

            // Giả định: mỗi short code trỏ tới 1 URL như sau
            var fullUrl = $"https://yourdomain.com/{code}";

            var imageBytes = _qrService.GenerateQrCode(fullUrl);
            return File(imageBytes, "image/png");
        }
    }
}
