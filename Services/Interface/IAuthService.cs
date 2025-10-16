using HR_Arvius.DTOs.Responses;
namespace HR_Arvius.Services.Interface
{
    public interface IAuthService
    {
        Task<AuthResponse> AuthenticateAsync(string Email, string Password);
        Task<AuthResponse> RefreshAsync(string RefreshToken);
    }
}
