using Core.Dto;

namespace Core.Interfaces
{
    public interface IJWTManager
    {
        Task<AuthResponseDto> GetTokenAsync(string primaryLoginId);
        Task<AuthResponseDto> SaveTokenDetails(string primaryLoginId, string tokenString, string refreshToken);
    }
}
