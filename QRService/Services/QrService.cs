using QRCoder;
using System;
using System.IO;

namespace QrService.Services
{
    public class QrCodeService
    {   
        public byte[] GenerateQrCode(string url)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentException("URL cannot be null or empty", nameof(url));
                
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
            using var pngByteQRCode = new PngByteQRCode(qrCodeData);
            return pngByteQRCode.GetGraphic(20);
        }
    }
}
