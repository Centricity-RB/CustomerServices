using Core.Dto;
using Core.Enums;
using Core.Interfaces;
using Core.Utility;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Reflection;

namespace Authentication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly ITwilioManager _twilioManager;
        private readonly IJWTManager _jWTManager;

        public AuthenticationController(
            ITwilioManager twilioManager,
            IJWTManager jWTManager)
        {
            _twilioManager = twilioManager;
            _jWTManager = jWTManager;
        }

        private string GetArea(string functionName)
        {
            var methodInfo = MethodBase.GetCurrentMethod();
            return $"{methodInfo.DeclaringType.FullName}.{methodInfo.Name}.{functionName}";
        }


        [Route("VerifyOtpToSignIn")]
        [HttpPost]
        public async Task<IActionResult> SignIn(ValidateLoginOtpDto login)
        {
            var attr = new Dictionary<string, object>
            {
                { "Area",GetArea("Create")}
            };
            try
            {
                if (login == null)
                    return StatusCode((int)HttpStatusCode.BadRequest, "Provide Login Request ");

                if (string.IsNullOrWhiteSpace(login.PrimaryLoginId))
                    return StatusCode((int)HttpStatusCode.BadRequest, "Provide Login id");

                if (login.SignInType == SignInType.PhoneNumber && string.IsNullOrWhiteSpace(login.CountryCode))
                    return StatusCode((int)HttpStatusCode.BadRequest, "Provide country code");
                
                var receiver = login.SignInType == SignInType.PhoneNumber ? login.CountryCode + login.PrimaryLoginId : login.PrimaryLoginId;
                var result = await _twilioManager.CheckVerificationAsync(receiver, login.SignInType, login.Otp);
                if (result.IsValid && !string.IsNullOrWhiteSpace(result.Sid))
                {
                    var authResponse = await _jWTManager.GetTokenAsync(login.PrimaryLoginId);
                    return Ok(authResponse);
                }
                else
                {
                    return Ok(new AuthResponseDto() { IsSuccess = false, Reason = Constants.Invalid_OTP });
                }
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        [Route("SendOtpToSignIn")]
        [HttpPost]
        public async Task<IActionResult> SendOtpToSignIn(LoginDto login)
        {
            var attr = new Dictionary<string, object>
            {

                { "Area",GetArea("Create")}
            };
            try
            {
                //return error
                if (login == null)
                    return StatusCode((int)HttpStatusCode.BadRequest, "Provide Login Request ");

                if (string.IsNullOrWhiteSpace(login.PrimaryLoginId))
                    return StatusCode((int)HttpStatusCode.BadRequest, "Provide Login id");

                if (login.SignInType == SignInType.PhoneNumber && string.IsNullOrWhiteSpace(login.CountryCode))
                    return StatusCode((int)HttpStatusCode.BadRequest, "Provide country code");

                var receiver = login.SignInType == SignInType.PhoneNumber ? login.CountryCode + login.PrimaryLoginId 
                    : login.PrimaryLoginId;
                attr.Add("PrimaryLoginId", receiver);
                var result = await _twilioManager.StartVerificationAsync(receiver, login.SignInType);
                if (result.IsValid && !string.IsNullOrWhiteSpace(result.Sid))
                {
                    return Ok(new OtpSendResponseDto() { IsSuccess = true, Reason = string.Empty });
                }
                return Ok(new OtpSendResponseDto() { IsSuccess = false, Reason = Constants.OTP_Send_Failed });
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}
