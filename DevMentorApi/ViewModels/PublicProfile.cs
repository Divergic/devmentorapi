namespace DevMentorApi.ViewModels
{
    using System;
    using DevMentorApi.Model;
    using EnsureThat;

    public class PublicProfile
    {
        public PublicProfile()
        {
        }

        public PublicProfile(Profile profile)
        {
            Ensure.That(profile, nameof(profile)).IsNotNull();

            AccountId = profile.AccountId;
            Email = profile.Email;
            FirstName = profile.FirstName;
            LastName = profile.LastName;
        }

        public Guid AccountId { get; set; }

        public string Email { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }
    }
}