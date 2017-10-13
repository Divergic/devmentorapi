namespace TechMentorApi.Azure
{
    using EnsureThat;
    using Model;

    public class AccountResult : Account
    {
        public AccountResult(Account source)
        {
            Ensure.That(source, nameof(source)).IsNotNull();

            Id = source.Id;
            Provider = source.Provider;
            Subject = source.Subject;
        }

        public bool IsNewAccount { get; set; }
    }
}