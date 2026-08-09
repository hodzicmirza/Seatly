using QRCoder;
using Seatly.Application.Interfaces;

namespace Seatly.Infrastructure.Services;

public class QrCodeService : IQrCodeService
{
    public Task<string> GenerateQrCodeAsync(string data)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);

        var qrCodeBytes = qrCode.GetGraphic(
            20,
            new byte[] { 45, 55, 72 }, // Dark blue
            new byte[] { 255, 255, 255 }
        ); // White

        var base64String = Convert.ToBase64String(qrCodeBytes);
        return Task.FromResult(base64String);
    }

    public Task<bool> ValidateQrCodeAsync(string qrCodeData)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(qrCodeData))
                return Task.FromResult(false);

            var bytes = Convert.FromBase64String(qrCodeData);
            return Task.FromResult(bytes.Length > 0);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }
}
