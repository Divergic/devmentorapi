namespace DevMentorApi.Security
{
    public interface IAuthenticationConfig
    {
        string Audience
        {
            get;
            set;
        }

        string Authority
        {
            get;
            set;
        }

        string SecretKey
        {
            get;
            set;
        }

        bool RequireHttps
        {
            get;
            set;
        }
    }
}