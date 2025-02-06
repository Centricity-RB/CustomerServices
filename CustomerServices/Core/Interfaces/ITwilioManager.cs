using Core.Dto;
using Core.Enums;

namespace Core.Interfaces
{
    public interface ITwilioManager
    {
        Task<VerificationResult> StartVerificationAsync(string phoneNumber, SignInType signInType);

        Task<VerificationResult> CheckVerificationAsync(string phoneNumber, SignInType signInType, string otp);

        Task<string> VerifyTokenAndResendOTP(string token, string actions);

        Task<string> CreateResendOTPToken(string primaryLoginId, string countryCode, int resendOTPCounter, int channel, string sid, string action, string clientId = "");

        Task InvalidateResendToken(string token);

        Task<bool> SendSMSNotification(string fromNumber, string toNumber, string messageBody);
    }
}
