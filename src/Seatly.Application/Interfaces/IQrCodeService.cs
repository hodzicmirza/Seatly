namespace Seatly.Application.Interfaces;

public interface IQrCodeService
{
    Task<string> GenerateQrCodeAsync(string data);
    Task<bool> ValidateQrCodeAsync(string qrCodeData);
}
