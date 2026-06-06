namespace SmartCart.Core.Interfaces.IServices.Application;

public interface IOtpService
{
    Task<string> GenerateAndSendOtpAsync(string mobileNumber);
    bool VerifyOtp(string mobileNumber, string otp);
    void InvalidateOtp(string mobileNumber);
}
