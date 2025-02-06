using Core.Dto;
using Core.Enums;
using Core.Interfaces;
using Core.Settings;
using Core.Utility;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using Twilio;
using Twilio.Exceptions;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Rest.Verify.V2.Service;

namespace Core.Managers
{
    public class TwilioManager : ITwilioManager
    {
        private readonly TwilioConfiguration _config;
        //private readonly IConstantsRepository _constantsRepository;

        public TwilioManager(IOptions<TwilioConfiguration> configuration)//, IConstantsRepository constantsRepository)
        {
            _config = configuration.Value;
            //_constantsRepository = constantsRepository;
            TwilioClient.Init(_config.AccountSid, _config.AuthToken);
        }
        private string GetArea(string functionName)
        {
            var methodInfo = MethodBase.GetCurrentMethod();
            return $"{methodInfo.DeclaringType.FullName}.{methodInfo.Name}.{functionName}";
        }

        public async Task<VerificationResult> StartVerificationAsync(string primaryLoginId, SignInType signInType)
        {
            var attr = new Dictionary<string, object> { { "Area", GetArea("StartVerificationAsync") }, 
                { "PrimaryLoginId", primaryLoginId } };
            try
            {
                var channel = signInType.GetType().GetField(signInType.ToString()).GetCustomAttribute<DescriptionAttribute>().Description;
                var verificationResource = await VerificationResource.CreateAsync(
                    to: primaryLoginId,
                    channel: channel,
                    pathServiceSid: _config.VerificationSid
                );
                return new VerificationResult(verificationResource.Sid);
            }
            catch (TwilioException e)
            {
                return new VerificationResult(new List<string> { e.Message });
            }
        }
        public async Task<string> CreateResendOTPToken(string primaryLoginId, string countryCode, int resendOTPCounter, int channel, string sid, string action, string clientId = "")
        {

            var attr = new Dictionary<string, object> { { "Area", GetArea("CreateResendOTPToken") }, { "PrimaryLoginId", primaryLoginId } };

            try
            {

                var jwtKey = _config.TokenKey;
                var keyBytes = Encoding.ASCII.GetBytes(jwtKey);

                var tokenHandler = new JwtSecurityTokenHandler();

                var descriptor = new SecurityTokenDescriptor()
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                    new Claim(ClaimTypes.NameIdentifier, primaryLoginId),
                    new Claim(ClaimTypes.UserData,action)

                    }),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes),
                   SecurityAlgorithms.HmacSha256)
                };

                var token = tokenHandler.CreateToken(descriptor);
                string tokenString = tokenHandler.WriteToken(token);

                //save the token in DB

                //var otpRequest = new Otprequest
                //{
                //    CreatedDate = DateTime.UtcNow,
                //    ExpirationDate = DateTime.UtcNow.AddMinutes(_config.ResendOTPTokenExpiration),
                //    IsInvalidated = false,
                //    Token = tokenString,
                //    PrimaryLoginId = primaryLoginId,
                //    Count = resendOTPCounter,
                //    Sid = sid,
                //    CountryCode = countryCode,
                //    Channel = channel,
                //    ClientId = clientId,
                //    Action = action
                //};
                //await _context.Otprequests.AddAsync(otpRequest);
                //await _context.SaveChangesAsync().ConfigureAwait(false);

                return tokenString;

            }
            catch (TwilioException e)
            {
                return null;
            }

        }
        public async Task<string> VerifyTokenAndResendOTP(string token, string actions)
        {
            var attr = new Dictionary<string, object> { { "Area", GetArea("VerifyTokenAndResendOTP") } };

            try
            {
                var keyBytes = Encoding.ASCII.GetBytes(_config.TokenKey);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateLifetime = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
                    ValidateAudience = false,
                    ValidateIssuer = false

                };
                ISecurityTokenValidator tokenValidator = new JwtSecurityTokenHandler();
                var claim = tokenValidator.ValidateToken(token, validationParameters, out var _);
                string primaryLoginId = claim.FindFirst(c => c.Type.ToLower() == ClaimTypes.NameIdentifier)!.Value;
                string action = claim.FindFirst(c => c.Type.ToLower() == ClaimTypes.UserData)!.Value;
                if (string.IsNullOrWhiteSpace(primaryLoginId) || string.IsNullOrWhiteSpace(action))
                    return null;

                //var resendValue = await _constantsRepository.GetConstantValueFromCache(Constants.ResendOtpMaxAttempts);
                //if (string.IsNullOrWhiteSpace(resendValue))
                //{
                //    return null;
                //}

                //var otpRequest = await _context.Otprequests.FirstOrDefaultAsync(
                //    x => x.IsInvalidated == false && x.Token == token
                //    && x.ExpirationDate > DateTime.UtcNow
                //    && x.CreatedDate.AddSeconds(_config.ResentOTPTokenActivation) <= DateTime.UtcNow
                //    && x.Count < Convert.ToInt32(resendValue)
                //    && x.PrimaryLoginId == primaryLoginId
                //    && x.Action == action

                //    );

                //if (otpRequest == null || !actions.Contains(action)) return null;

                //VerificationResult verificationResult = null;
                //if (otpRequest.Channel == Constants.PhoneChannel)//mobile
                //    verificationResult = await StartVerificationAsync(otpRequest.CountryCode + otpRequest.PrimaryLoginId, "sms");
                //else if (otpRequest.Channel == Constants.EmailChannel)//email
                //    verificationResult = await StartVerificationAsync(otpRequest.PrimaryLoginId, "email");
                //if (verificationResult == null || !verificationResult.IsValid)
                //    return null;

                ////invalidate current tooken
                //otpRequest.IsInvalidated = true;
                //_context.Otprequests.Update(otpRequest);
                //await _context.SaveChangesAsync();

                ////create new token
                //var newToken = await CreateResendOTPToken(otpRequest.PrimaryLoginId, otpRequest.CountryCode, otpRequest.Count + 1, otpRequest.Channel, verificationResult.Sid, action);

                return string.Empty; //newToken;
            }
            catch (TwilioException e)
            {
                return null;
            }
        }
        public async Task InvalidateResendToken(string token)
        {
            var attr = new Dictionary<string, object> { { "Area", GetArea("InvalidateResendToken") } };

            try
            {
                if (string.IsNullOrWhiteSpace(token)) return;
                //var otpRequest = await _context.Otprequests.FirstOrDefaultAsync(
                //    x => x.IsInvalidated == false && x.Token == token);
                ////invalidate current tooken
                //if (otpRequest == null) return;
                //otpRequest.IsInvalidated = true;
                //_context.Otprequests.Update(otpRequest);
                //await _context.SaveChangesAsync();
            }
            catch (TwilioException e)
            {
                //_logger.LogException(e, attr);
            }
        }
        public async Task<VerificationResult> CheckVerificationAsync(string primaryLoginId, SignInType signInType, string otp)
        {
            var attr = new Dictionary<string, object> { { "Area", GetArea("CheckVerificationAsync") }, { "PrimaryLoginId", primaryLoginId } };
            try
            {
                var verificationCheckResource = await VerificationCheckResource.CreateAsync(
                    to: primaryLoginId,
                    code: otp,
                    pathServiceSid: _config.VerificationSid
                );
                return verificationCheckResource.Status.Equals("approved") ?
                    new VerificationResult(verificationCheckResource.Sid) :
                    new VerificationResult(new List<string> { "Wrong code. Try again." });
            }
            catch (TwilioException e)
            {
                return new VerificationResult(new List<string> { e.Message });
            }
        }

        public async Task<bool> SendSMSNotification(string fromNumber, string toNumber, string messageBody)
        {
            bool result = false;
            try
            {
                //_logger.Info($"Sending SMS to {toNumber} with {messageBody}.");
                if (string.IsNullOrWhiteSpace(fromNumber))
                {
                    //_logger.Error($"SMS sent failed. From number is required.");
                    return false;
                }
                var message = MessageResource.Create(
                    body: messageBody,
                    from: fromNumber,
                    to: new Twilio.Types.PhoneNumber(toNumber)
                );
                if (message.Status == MessageResource.StatusEnum.Undelivered || message.Status == MessageResource.StatusEnum.Failed || message.Status == MessageResource.StatusEnum.Canceled)
                {
                    // _logger.Info($"SMS sent failed to {toNumber}.Status Code -{message.Status}");
                }
                else
                {
                    //_logger.Info($"SMS sent successfully to {toNumber}. Status Code -{message.Status}");
                    result = true;
                }
                return result;
            }
            catch (TwilioException ex)
            {
                //_logger.Info($"SMS sent failed to {toNumber}.");
                //_logger.LogException(ex);
                return false;
            }
            catch (Exception e)
            {
                //_logger.Info($"SMS sent failed to {toNumber}.");
                //_logger.LogException(e);
                return false;
            }
        }
    }
}
