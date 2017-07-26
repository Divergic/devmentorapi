namespace DevMentorApi.Model
{
    using System;
    using EnsureThat;

    public class Account : NewAccount
    {
        public Account()
        {
        }

        public Account(NewAccount account)
        {
            Ensure.That(account, nameof(account)).IsNotNull();

            Id = account.Id;
            Provider = account.Provider;
            Username = account.Username;
        }

        public DateTimeOffset? BannedAt
        {
            get;
            set;
        }
    }
}