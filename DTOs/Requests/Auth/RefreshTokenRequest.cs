namespace HR_Arvius.DTOs.Requests.Auth
{
    public class RefreshTokenRequest
    {
        public string RefreshToken { get; set; }
        public DateTime RefreshTokenExpiryTime {  get; set; }
    }
}
