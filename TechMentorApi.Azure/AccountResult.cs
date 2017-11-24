namespace TechMentorApi.Azure
{
    using EnsureThat;
    using Model;

    public class AccountResult : Account
    {
        public AccountResult(Account source)
        {
            Ensure.Any.IsNotNull(source, nameof(source));

            Id = source.Id;
            Provider = source.Provider;
            Subject = source.Subject;
        }

        public bool IsNewAccount { get; set; }
    }
}