namespace DevMentorApi
{
    using DevMentorApi.Azure;
    using DevMentorApi.Security;

    public class Config
    {
        public AuthenticationConfig Authentication
        {
            get;
            set;
        }

        public StorageConfiguration Storage
        {
            get;
            set;
        }
    }
}