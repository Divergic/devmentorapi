﻿namespace DevMentorApi.Business
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using DevMentorApi.Model;

    public interface IProfileManager
    {
        Task BanProfile(Guid id, DateTimeOffset bannedAt, CancellationToken cancellationToken);

        Task<Profile> GetProfile(Guid id, CancellationToken cancellationToken);

        Task UpdateProfile(Profile profile, CancellationToken cancellationToken);
    }
}