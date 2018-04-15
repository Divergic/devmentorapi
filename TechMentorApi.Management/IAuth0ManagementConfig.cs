namespace TechMentorApi.Management
{
    public interface IAuth0ManagementConfig
    {
        string Audience { get; }
        string ClientId { get; }
        string ClientSecret { get; }
        bool IsEnabled { get; }
        string Tenant { get; }
    }
}