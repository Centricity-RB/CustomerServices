using Core.Dto;
using Core.Interfaces;
using Core.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Core.Managers
{
    public class JWTManager : IJWTManager
    {
        private readonly JwtSettings _configuration;

        public JWTManager(IOptions<JwtSettings> configuration)
        {
            //_context = context;
            _configuration = configuration.Value;
        }
        private string GetArea(string functionName)
        {
            var methodInfo = MethodBase.GetCurrentMethod();
            return $"{methodInfo.DeclaringType.FullName}.{methodInfo.Name}.{functionName}";
        }


        public async Task<AuthResponseDto> GetTokenAsync(string primaryLoginId)
        {
            var attr = new Dictionary<string, object>
            {
                { "PrimaryLoginId",primaryLoginId},
                { "Area",GetArea("GetTokenAsync")}
            };

            try
            {
                string tokenString = GenerateToken(primaryLoginId);
                string refreshToken = GenerateRefreshToken();
                return await SaveTokenDetails(primaryLoginId, tokenString, refreshToken);
            }
            catch (Exception ex)
            {
                return new AuthResponseDto { IsSuccess = false, Reason = "Unable to create token" };
            }

        }

        public async Task<AuthResponseDto> SaveTokenDetails(string primaryLoginId, string tokenString, string refreshToken)
        {
            var attr = new Dictionary<string, object>
            {
                { "PrimaryLoginId",primaryLoginId},
                { "Area",GetArea("GetTokenAsync")}
            };
            try
            {
                //var userRefreshToken = new UserRefreshToken
                //{
                //    CreatedDate = DateTime.UtcNow,
                //    ExpirationDate = DateTime.UtcNow.AddMinutes(_configuration.TokenExpiration),
                //    IsInvalidated = false,
                //    IsActive = true,
                //    RefreshToken = refreshToken,
                //    Token = tokenString,
                //    PrimaryLoginId = primaryLoginId
                //};
                //await _context.UserRefreshTokens.AddAsync(userRefreshToken);
                //await _context.SaveChangesAsync().ConfigureAwait(false);
                return new AuthResponseDto { Token = tokenString, RefreshToken = refreshToken, IsSuccess = true };
            }
            catch (Exception ex)
            {
                return new AuthResponseDto { IsSuccess = false, Reason = "Unable to create token" };
            }
        }

        private string GenerateRefreshToken()
        {
            var byteArray = new byte[64];

            using (var randomNumberGenerator = RandomNumberGenerator.Create())
            {
                randomNumberGenerator.GetBytes(byteArray);
                return Convert.ToBase64String(byteArray);
            }
        }

        private string GenerateToken(string userName)
        {
            var jwtKey = _configuration.Key;
            var keyBytes = Encoding.ASCII.GetBytes(jwtKey);

            var tokenHandler = new JwtSecurityTokenHandler();

            var descriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userName)

                }),

                Issuer = _configuration.Issuer,
                Audience = _configuration.Audience,


                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes),
               SecurityAlgorithms.HmacSha256)
            };

            var token = tokenHandler.CreateToken(descriptor);
            string tokenString = tokenHandler.WriteToken(token);
            return tokenString;
        }

        public async Task<bool> IsTokenValid(string accessToken)
        {
            //var isValid = _context.UserRefreshTokens.FirstOrDefault(x => x.Token == accessToken) != null;
            //return await Task.FromResult(isValid);
            return true;
        }

        public string GetUserFromToken(string jwtToken)
        {
            var keyBytes = Encoding.ASCII.GetBytes(_configuration.Key);
            string primaryLoginId = "";
            var validationParameters = new TokenValidationParameters
            {
                // RequireExpirationTime = true, validating aginsts db
                ValidateLifetime = false,
                //ClockSkew = TimeSpan.Zero,
                //RequireSignedTokens = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
                ValidateIssuer = true,
                ValidIssuer = _configuration.Issuer,
                ValidateAudience = true,
                ValidAudience = _configuration.Audience
            };
            ISecurityTokenValidator tokenValidator = new JwtSecurityTokenHandler();
            var claim = tokenValidator.ValidateToken(jwtToken, validationParameters, out var _);
            return claim.FindFirst(c => c.Type.ToLower() == ClaimTypes.NameIdentifier)!.Value;

        }

        public Task<string> GetClaimValueFromToken(string jwtToken, string claimKey)
        {
            var result = "";
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var ssoTokenDetail = tokenHandler.ReadJwtToken(jwtToken);
                result = ssoTokenDetail.Claims.FirstOrDefault(x => x.Type == claimKey)?.Value;

            }
            catch (Exception ex)
            {

            }
            return Task.FromResult(result);
        }
        public async Task<AuthResponseDto> GenerateTokenForMicrosoftSSO(string jwtToken)
        {
            AuthResponseDto tokenResponse = null;
            try
            {

                var jwtKey = _configuration.Key;
                var keyBytes = Encoding.ASCII.GetBytes(jwtKey);

                var tokenHandler = new JwtSecurityTokenHandler();
                var ssoTokenDetail = tokenHandler.ReadJwtToken(jwtToken);
                var tid = ssoTokenDetail.Claims.FirstOrDefault(x => x.Type == "tid")?.Value;
                var clientId = ssoTokenDetail.Claims.FirstOrDefault(x => x.Type == "aud")?.Value;
                var issuer = ssoTokenDetail.Claims.FirstOrDefault(x => x.Type == "aud")?.Value;
                var email = ssoTokenDetail.Claims.FirstOrDefault(x => x.Type == "email")?.Value;
                var issuerValue = ssoTokenDetail.Claims.FirstOrDefault(x => x.Type == "iss")?.Value;
                var descriptor = new SecurityTokenDescriptor()
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                        new Claim("tid", tid),
                        new Claim("clientid",clientId),
                        new Claim("sub",ssoTokenDetail?.Subject),
                        new Claim("email",email),
                        new Claim("originalAuthIssuer",issuerValue)

                    }),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes),
                   SecurityAlgorithms.HmacSha256)
                };

                var token = tokenHandler.CreateToken(descriptor);
                string refreshToken = GenerateRefreshToken();
                tokenResponse = await SaveTokenDetails(email, tokenHandler.WriteToken(token), refreshToken);
            }
            catch (Exception e)
            {
                //_capLogger.LogException(e);
            }
            return tokenResponse;

        }
        private JwtSecurityToken GetJwtToken(string expiredToken)
        {
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            //tokenHandler.ValidateToken
            return tokenHandler.ReadJwtToken(expiredToken);
        }
        //private AuthResponseDto ValidateDetails(JwtSecurityToken token, UserRefreshToken userRefreshToken)
        //{
        //    if (userRefreshToken == null)
        //        return new AuthResponseDto { IsSuccess = false, Reason = "Invalid Token Details." };
        //    if (userRefreshToken.ExpirationDate > DateTime.UtcNow)
        //        return new AuthResponseDto { IsSuccess = false, Reason = "Token not expired." };
        //    if (!userRefreshToken.IsActive.Value)
        //        return new AuthResponseDto { IsSuccess = false, Reason = "Refresh Token Expired" };
        //    return new AuthResponseDto { IsSuccess = true };
        //}


        public async Task<bool> InvalidateToken(string authorization)
        {
            var attr = new Dictionary<string, object> { { "Area", GetArea("InvalidateToken") } };
            try
            {
                var primaryLoginId = GetUserFromToken(authorization);
                attr.Add("PrimaryLoginId", primaryLoginId);

                var token = GetJwtToken(authorization);
                //var userRefreshToken = _context.UserRefreshTokens.FirstOrDefault(x => x.IsInvalidated == false
                //&& x.Token == authorization && x.PrimaryLoginId == primaryLoginId);
                //if (userRefreshToken == null) return false;

                //invalidate existing token..
               // userRefreshToken.IsInvalidated = true;
                //userRefreshToken.ExpirationDate = DateTime.UtcNow.AddDays(-1);
                //_context.UserRefreshTokens.Update(userRefreshToken);
                //await _context.SaveChangesAsync();
                return true;


            }
            catch (Exception ex)
            {
                //_capLogger.LogException(ex, attr);
                return false;
            }


        }
    }
}
