using Core.Enums;

namespace Core.Dto
{
    public class LoginDto
    {
        public string PrimaryLoginId { get; set; }
        public SignInType SignInType { get; set; }
        public string CountryCode { get; set; }
    }

    public class ValidateLoginOtpDto : LoginDto
    {
        public string Otp { get; set; }
    }

    public class AuthResponseDto
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public bool IsSuccess { get; set; }
        public string Reason { get; set; }
    }

    public class OtpSendResponseDto
    {
        public bool IsSuccess { get; set; }
        public string Reason { get; set; }
    }
}
