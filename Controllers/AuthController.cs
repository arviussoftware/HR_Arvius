using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using HR_Arvius.Services.Interface;
using HR_Arvius.DTOs.Requests.Auth;

namespace HR_Arvius.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var response = await _authService.AuthenticateAsync(request.Email, request.Password);
            if (response == null)
                return Unauthorized("Invalid Credentials.");

            Response.Cookies.Append("accessToken", response.AccessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = false, 
                SameSite = SameSiteMode.Lax,
                Expires = response.AccessTokenExpiry
            });

            var now = DateTime.UtcNow;

            return Ok(new
            {
                status = "success",
                accessToken = response.AccessToken,
                accessTokenExpiry = (int)(response.AccessTokenExpiry - now).TotalSeconds,
                refreshToken = response.RefreshToken,
                user = new
                {
                    id = response.Id,
                    email = response.Email,
                    firstName = response.FirstName,
                    lastName = response.LastName,
                    employee_id = response.Employee_id,
                    role = response.Role,
                    department = response.Department,
                    location = response.Location
                }
            });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
        {
            var refreshTokenFromSession = HttpContext.Session.GetString("RefreshToken");

            if (string.IsNullOrEmpty(refreshTokenFromSession) || refreshTokenFromSession != request.RefreshToken)
            {
                return Unauthorized("Invalid or expired refresh token.");
            }

            var response = await _authService.RefreshAsync(request.RefreshToken);
            if (response == null)
                return Unauthorized("Invalid or expired refresh token.");

            HttpContext.Session.SetString("AccessToken", response.AccessToken);
            HttpContext.Session.SetString("RefreshToken", response.RefreshToken);

            var now = DateTime.UtcNow;

            return Ok(new
            {
                accessToken = response.AccessToken,
                accessTokenExpiry = (int)(response.AccessTokenExpiry - now).TotalSeconds,
                refreshToken = response.RefreshToken,
                refreshTokenExpiry = (int)(response.RefreshTokenExpiryTime - now).TotalSeconds
            });
        }

        [HttpGet("test")]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public IActionResult Test()
        {
            return Ok("You accessed a protected endpoint!");
        }
    }
}
