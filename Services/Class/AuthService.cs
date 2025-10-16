namespace HR_Arvius.Services.Class
{
    using Npgsql;
    using HR_Arvius.Helpers;
    using HR_Arvius.DTOs.Responses;
    using System.Security.Claims;
    using Microsoft.Extensions.Configuration;
    using HR_Arvius.Services.Interface;

    public class AuthService : IAuthService
    {
        private readonly IConfiguration _config;

        private static Dictionary<string, string> refreshTokens = new();

        public AuthService(IConfiguration config)
        {
            _config = config;
        }

        public async Task<AuthResponse> AuthenticateAsync(string email, string password)
        {
            string connectionString = _config.GetConnectionString("DefaultConnection");
            string storedHash = null;
            string emailFromDb = null;
            long userId = 0;
            string firstName = null;
            string lastName = null;
            string employee_id = null;
            string role = null;
            string department = null;
            string location = null;

            using (var connection = new NpgsqlConnection(connectionString))
            {
                await connection.OpenAsync();
                var cmd = new NpgsqlCommand("SELECT * FROM user_data_fetch(@Email)", connection);
                cmd.Parameters.AddWithValue("Email", email);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    userId = reader.GetInt64(0);  
                    emailFromDb = reader.GetString(1);
                    storedHash = reader.GetString(2); 
                    firstName = reader.IsDBNull(3) ? null : reader.GetString(3);
                    lastName = reader.IsDBNull(4) ? null : reader.GetString(4);
                    employee_id = reader.IsDBNull(5) ? null : reader.GetString(5);
                    role = reader.IsDBNull(6) ? null : reader.GetString(6);
                    department = reader.IsDBNull(7) ? null : reader.GetString(7);
                    location = reader.IsDBNull(8) ? null : reader.GetString(8);
                }
                else
                {
                    return null;  // User not found
                }
            }

            // Verify password after closing DB connection
            if (!BCrypt.Net.BCrypt.Verify(password, storedHash))
            {
                return null; // Invalid credentials
            }

            var accessExpiry = DateTime.UtcNow.AddMinutes(int.Parse(_config["Jwt:AccessTokenExpiryMinutes"]));
            var refreshExpiry = DateTime.UtcNow.AddDays(int.Parse(_config["Jwt:RefreshTokenExpiryDays"]));

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, emailFromDb),
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, role)
            };

            var accessToken = JwtHelper.GenerateToken(
                claims,
                _config["Jwt:SecretKey"],
                _config["Jwt:Issuer"],
                _config["Jwt:Audience"],
                accessExpiry
            );

            // Generate new refresh token and store it in memory dictionary
            var refreshToken = Guid.NewGuid().ToString();

            // Save refresh token in memory, linked by user email or userId as key
            refreshTokens[emailFromDb] = refreshToken;

            return new AuthResponse
            {
                AccessToken = accessToken,
                AccessTokenExpiry = accessExpiry,
                RefreshToken = refreshToken,
                RefreshTokenExpiryTime = refreshExpiry,
                Id  = userId,
                Email = emailFromDb,
                FirstName = firstName,
                LastName = lastName,
                Employee_id = employee_id,
                Role = role,
                Department = department,
                Location = location
            };
        }

        public async Task<AuthResponse> RefreshAsync(string refreshToken)
        {
            // Try to find user email that corresponds to this refresh token in in-memory store
            var email = refreshTokens.FirstOrDefault(x => x.Value == refreshToken).Key;

            if (email == null)
            {
                return null;  // Refresh token invalid or expired (not found in memory)
            }

            string connectionString = _config.GetConnectionString("DefaultConnection");
            long userId = 0;

            using (var connection = new NpgsqlConnection(connectionString))
            {
                await connection.OpenAsync();
                var cmd = new NpgsqlCommand("SELECT id FROM users WHERE email = @Email", connection);
                cmd.Parameters.AddWithValue("Email", email);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    userId = reader.GetInt64(0);
                }
                else
                {
                    return null;  // User not found
                }
            }

            var accessExpiry = DateTime.UtcNow.AddMinutes(int.Parse(_config["Jwt:AccessTokenExpiryMinutes"]));
            var refreshExpiry = DateTime.UtcNow.AddDays(int.Parse(_config["Jwt:RefreshTokenExpiryDays"]));

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            };

            var accessToken = JwtHelper.GenerateToken(
                claims,
                _config["Jwt:SecretKey"],
                _config["Jwt:Issuer"],
                _config["Jwt:Audience"],
                accessExpiry
            );

            var newRefreshToken = Guid.NewGuid().ToString();

            refreshTokens[email] = newRefreshToken;

            return new AuthResponse
            {
                AccessToken = accessToken,
                AccessTokenExpiry = accessExpiry,
                RefreshToken = newRefreshToken,
                RefreshTokenExpiryTime = refreshExpiry
            };
        }
    }
}
