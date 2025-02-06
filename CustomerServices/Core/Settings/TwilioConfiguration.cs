namespace Core.Settings
{
    public class TwilioConfiguration
    {
        public string AccountSid { get; set; } = "ACXXXXXXXXXXXXXXXXXXXXXXXXXXXX";
        public string AuthToken { get; set; } = "aXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX";
        public string VerificationSid { get; set; } = "VAXXXXXXXXXXXXXXXXXXXXXXXXXXXX";
        public string TokenKey { get; set; }

        public int ResentOTPTokenActivation { get; set; }

        public int ResendOTPMaxAttempts { get; set; }

        public int ResendOTPTokenExpiration { get; set; }
        public string SendGridClientKey { get; set; }
    }
}
