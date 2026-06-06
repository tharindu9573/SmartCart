using System.Collections.Concurrent;
using SmartCart.Core.Interfaces.IServices.Application;

namespace SmartCart.Infrastructure.Services;

public class OtpService : IOtpService
{
    private readonly INotificationService _notificationService;
    private readonly ConcurrentDictionary<string, (string Otp, DateTime ExpiresAt)> _otpStore = new();

    public OtpService(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task<string> GenerateAndSendOtpAsync(string mobileNumber)
    {
        var otp = new Random().Next(100000, 999999).ToString();
        _otpStore[mobileNumber] = (otp, DateTime.UtcNow.AddMinutes(5));
        await _notificationService.SendOtpAsync(mobileNumber, otp);
        return otp;
    }

    public bool VerifyOtp(string mobileNumber, string otp)
    {
        if (!_otpStore.TryRemove(mobileNumber, out var stored)) return false;
        if (DateTime.UtcNow > stored.ExpiresAt) return false;
        return stored.Otp == otp;
    }

    public void InvalidateOtp(string mobileNumber) => _otpStore.TryRemove(mobileNumber, out _);
}
