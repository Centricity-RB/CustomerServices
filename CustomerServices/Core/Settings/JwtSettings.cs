namespace Core.Settings
{
    public class JwtSettings
    {
        public string Key { get; set; }
        public string Issuer { get; set; }

        public string Audience { get; set; }

        public int TokenExpiration { get; set; }

        public int ActionTokenExpiration { get; set; }
        public string Issued_To { get; set; }
        public string subP { get; set; }
    }
}
