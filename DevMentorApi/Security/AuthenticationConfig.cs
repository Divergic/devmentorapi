namespace DevMentorApi.Security
{
    public class AuthenticationConfig : IAuthenticationConfig
    {
        public string Audience
        {
            get;
            set;
        }

        public string Authority
        {
            get;
            set;
        }

        public bool RequireHttps
        {
            get;
            set;
        }

        public string SecretKey
        {
            get;
            set;
        }
    }
}