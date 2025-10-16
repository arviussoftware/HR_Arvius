namespace HR_Arvius.Configuration
{
    public class JwtSettings
    {
        public string SecretKey { get; set; }  // Secret key for signing JWT
        public string Issuer { get; set; }     // Issuer (usually the app name)
        public string Audience { get; set; }   // Audience (who is the target for the token)
        public int AccessTokenExpiryMinutes { get; set; }
        public int RefreshTokenExpiryDays { get; set; }
    }

}
