using TechMentorApi.Management;

namespace TechMentorApi
{
    public class Auth0ManagementConfig : IAuth0ManagementConfig
    {
        public string Audience { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public bool IsEnabled { get; set; }
        public string Tenant { get; set; }
    }
}