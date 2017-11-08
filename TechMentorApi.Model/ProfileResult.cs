﻿namespace TechMentorApi.Model
{
    using System;
    using EnsureThat;

    public class ProfileResult
    {
        public ProfileResult()
        {
        }

        public ProfileResult(Profile profile)
        {
            Ensure.That(profile, nameof(profile)).IsNotNull();

            BirthYear = profile.BirthYear;
            FirstName = profile.FirstName;
            Gender = profile.Gender;
            Id = profile.Id;
            LastName = profile.LastName;
            Status = profile.Status;
            TimeZone = profile.TimeZone;
            YearStartedInTech = profile.YearStartedInTech;
        }

        public string AvatarETag { get; set; }

        public Guid? AvatarId { get; set; }

        public int? BirthYear { get; set; }

        public string FirstName { get; set; }

        public string Gender { get; set; }

        public Guid Id { get; set; }

        public string LastName { get; set; }

        public ProfileStatus Status { get; set; }

        public string TimeZone { get; set; }

        public int? YearStartedInTech { get; set; }
    }
}