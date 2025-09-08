using QRCoder;

namespace zellij.Services
{
    public interface IQrCodeService
    {
        string GenerateQrCodeDataUri(string text);
    }

    public class QrCodeService : IQrCodeService
    {
        public string GenerateQrCodeDataUri(string text)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);

            var qrCodeBytes = qrCode.GetGraphic(4);
            return $"data:image/png;base64,{Convert.ToBase64String(qrCodeBytes)}";
        }
    }
}