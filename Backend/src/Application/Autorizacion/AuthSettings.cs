namespace Auth
{
    public class AuthSettings
    {
        public required string AuthorizationSecret { get; set; }
        public required string AuthenticationSecret { get; set; }
        public required string ConnectionString { get; set; }
    }


}