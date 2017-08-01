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
            FirstName = profile.FirstName;
            LastName = profile.LastName;
            Status = profile.Status;
        }

        public Guid AccountId { get; set; }
        
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public ProfileStatus Status { get; set; }
    }
}