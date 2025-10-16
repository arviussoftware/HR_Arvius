namespace HR_Arvius.DTOs.Responses
{
    public class AuthResponse
    {
        public string AccessToken { get; set; }
        public DateTime AccessTokenExpiry { get; set; }
        public string RefreshToken { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; }

        public long Id { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Employee_id { get; set; }
        public string Department {  get; set; }
        public string Location { get; set; }
    }
}
