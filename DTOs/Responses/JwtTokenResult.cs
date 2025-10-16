namespace HR_Arvius.DTOs.Responses
{
    public class JwtTokenResult
    {
        public string Token { get; set; }
        public DateTime Expiry { get; set; }
        public string RefreshToken { get; set; }
    }
}
