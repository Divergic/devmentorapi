﻿namespace TechMentorApi.Business.UnitTests.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using ModelBuilder;
    using NSubstitute;
    using TechMentorApi.Azure;
    using TechMentorApi.Business.Commands;
    using TechMentorApi.Model;
    using Xunit;

    public class ProfileCommandTests
    {
        [Fact]
        public async Task BanProfileCallsStoreWithBanInformationTest()
        {
            var profile = Model.Create<Profile>();
            var bannedAt = DateTimeOffset.UtcNow.AddDays(-2);

            var store = Substitute.For<IProfileStore>();
            var calculator = Substitute.For<IProfileChangeCalculator>();
            var processor = Substitute.For<IProfileChangeProcessor>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new ProfileCommand(store, calculator, processor, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                calculator.RemoveAllCategoryLinks(profile).Returns(new ProfileChangeResult());
                store.BanProfile(profile.Id, Arg.Any<DateTimeOffset>(), tokenSource.Token).Returns(profile);

                await sut.BanProfile(profile.Id, bannedAt, tokenSource.Token).ConfigureAwait(false);

                await store.Received().BanProfile(profile.Id, bannedAt, tokenSource.Token).ConfigureAwait(false);
            }
        }

        [Fact]
        public async Task BanProfileDoesNotCacheProfileResultWhenNoResultsAreCachedTest()
        {
            var profile = Model.Create<Profile>().Set(x => x.BannedAt = DateTimeOffset.UtcNow);

            var store = Substitute.For<IProfileStore>();
            var calculator = Substitute.For<IProfileChangeCalculator>();
            var processor = Substitute.For<IProfileChangeProcessor>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new ProfileCommand(store, calculator, processor, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                calculator.RemoveAllCategoryLinks(profile).Returns(new ProfileChangeResult());
                store.BanProfile(profile.Id, profile.BannedAt.Value, tokenSource.Token).Returns(profile);
                cache.GetProfileResults().Returns((ICollection<ProfileResult>) null);

                await sut.BanProfile(profile.Id, profile.BannedAt.Value, tokenSource.Token).ConfigureAwait(false);

                cache.DidNotReceive().StoreProfileResults(Arg.Any<ICollection<ProfileResult>>());
            }
        }

        [Fact]
        public async Task BanProfileDoesNotProcessChangesWhenNoneFoundTest()
        {
            var profile = Model.Create<Profile>();
            var bannedAt = DateTimeOffset.UtcNow.AddDays(-2);

            var store = Substitute.For<IProfileStore>();
            var calculator = Substitute.For<IProfileChangeCalculator>();
            var processor = Substitute.For<IProfileChangeProcessor>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new ProfileCommand(store, calculator, processor, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                calculator.RemoveAllCategoryLinks(profile).Returns(new ProfileChangeResult());
                store.BanProfile(profile.Id, Arg.Any<DateTimeOffset>(), tokenSource.Token).Returns(profile);

                await sut.BanProfile(profile.Id, bannedAt, tokenSource.Token).ConfigureAwait(false);

                await processor.DidNotReceive().Execute(
                    Arg.Any<Profile>(),
                    Arg.Any<ProfileChangeResult>(),
                    Arg.Any<CancellationToken>()).ConfigureAwait(false);
            }
        }

        [Fact]
        public async Task BanProfileRemovesCategoryLinksTest()
        {
            var profile = Model.Create<Profile>();
            var changeResult = Model.Create<ProfileChangeResult>();
            var bannedAt = DateTimeOffset.UtcNow.AddDays(-2);

            var store = Substitute.For<IProfileStore>();
            var calculator = Substitute.For<IProfileChangeCalculator>();
            var processor = Substitute.For<IProfileChangeProcessor>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new ProfileCommand(store, calculator, processor, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                calculator.RemoveAllCategoryLinks(profile).Returns(changeResult);
                store.BanProfile(profile.Id, Arg.Any<DateTimeOffset>(), tokenSource.Token).Returns(profile);

                await sut.BanProfile(profile.Id, bannedAt, tokenSource.Token).ConfigureAwait(false);

                await processor.Received().Execute(profile, changeResult, tokenSource.Token).ConfigureAwait(false);
            }
        }

        [Fact]
        public async Task BanProfileRemovesProfileFromCacheTest()
        {
            var expected = Model.Create<Profile>().Set(x => x.Status = ProfileStatus.Unavailable);
            var bannedAt = DateTimeOffset.UtcNow.AddDays(-2);

            var store = Substitute.For<IProfileStore>();
            var calculator = Substitute.For<IProfileChangeCalculator>();
            var processor = Substitute.For<IProfileChangeProcessor>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new ProfileCommand(store, calculator, processor, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                calculator.RemoveAllCategoryLinks(expected).Returns(new ProfileChangeResult());
                store.BanProfile(expected.Id, bannedAt, tokenSource.Token).Returns(expected);

                await sut.BanProfile(expected.Id, bannedAt, tokenSource.Token).ConfigureAwait(false);

                cache.Received().RemoveProfile(expected.Id);
            }
        }

        [Fact]
        public async Task BanProfileRemovesProfileFromResultsCacheTest()
        {
            var profile = Model.Create<Profile>().Set(x => x.BannedAt = DateTimeOffset.UtcNow);
            var cacheResults = new List<ProfileResult>
            {
                Model.Create<ProfileResult>().Set(x => x.Id = profile.Id)
            };

            var store = Substitute.For<IProfileStore>();
            var calculator = Substitute.For<IProfileChangeCalculator>();
            var processor = Substitute.For<IProfileChangeProcessor>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new ProfileCommand(store, calculator, processor, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                calculator.RemoveAllCategoryLinks(profile).Returns(new ProfileChangeResult());
                store.BanProfile(profile.Id, profile.BannedAt.Value, tokenSource.Token).Returns(profile);
                cache.GetProfileResults().Returns(cacheResults);

                await sut.BanProfile(profile.Id, profile.BannedAt.Value, tokenSource.Token).ConfigureAwait(false);

                cache.Received().StoreProfileResults(
                    Verify.That<ICollection<ProfileResult>>(x => x.Should().NotContain(y => y.Id == profile.Id)));
            }
        }

        [Fact]
        public async Task BanProfileReturnsNullWhenProfileNotInStoreTest()
        {
            var profileId = Guid.NewGuid();
            var bannedAt = DateTimeOffset.UtcNow.AddDays(-2);

            var store = Substitute.For<IProfileStore>();
            var calculator = Substitute.For<IProfileChangeCalculator>();
            var processor = Substitute.For<IProfileChangeProcessor>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new ProfileCommand(store, calculator, processor, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                var actual = await sut.BanProfile(profileId, bannedAt, tokenSource.Token).ConfigureAwait(false);

                actual.Should().BeNull();
                cache.DidNotReceive().StoreProfile(Arg.Any<Profile>());
            }
        }

        [Fact]
        public void BanProfileThrowsExceptionWithEmptyIdTest()
        {
            var bannedAt = DateTimeOffset.UtcNow.AddDays(-2);

            var store = Substitute.For<IProfileStore>();
            var calculator = Substitute.For<IProfileChangeCalculator>();
            var processor = Substitute.For<IProfileChangeProcessor>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new ProfileCommand(store, calculator, processor, cache);

            Func<Task> action = async () => await sut.BanProfile(Guid.Empty, bannedAt, CancellationToken.None)
                .ConfigureAwait(false);

            action.ShouldThrow<ArgumentException>();
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullCacheTest()
        {
            var profileStore = Substitute.For<IProfileStore>();
            var calculator = Substitute.For<IProfileChangeCalculator>();
            var processor = Substitute.For<IProfileChangeProcessor>();

            Action action = () => new ProfileCommand(profileStore, calculator, processor, null);

            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullCalculatorTest()
        {
            var profileStore = Substitute.For<IProfileStore>();
            var processor = Substitute.For<IProfileChangeProcessor>();
            var cache = Substitute.For<ICacheManager>();

            Action action = () => new ProfileCommand(profileStore, null, processor, cache);

            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullProcessorTest()
        {
            var profileStore = Substitute.For<IProfileStore>();
            var calculator = Substitute.For<IProfileChangeCalculator>();
            var cache = Substitute.For<ICacheManager>();

            Action action = () => new ProfileCommand(profileStore, calculator, null, cache);

            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullProfileStoreTest()
        {
            var cache = Substitute.For<ICacheManager>();
            var calculator = Substitute.For<IProfileChangeCalculator>();
            var processor = Substitute.For<IProfileChangeProcessor>();

            Action action = () => new ProfileCommand(null, calculator, processor, cache);

            action.ShouldThrow<ArgumentNullException>();
        }

        [Theory]
        [InlineData(ProfileStatus.Available)]
        [InlineData(ProfileStatus.Unavailable)]
        public async Task UpdateProfileAddsProfileToResultsCacheWhenNotPreviouslyCachedTest(ProfileStatus status)
        {
            var expected = Model.Create<UpdatableProfile>().Set(x => x.Status = status);
            var profile = Model.Create<Profile>().Set(x => x.BannedAt = null);
            var changeResult = Model.Create<ProfileChangeResult>().Set(x => x.ProfileChanged = true);
            var cacheResults = new List<ProfileResult>();

            var store = Substitute.For<IProfileStore>();
            var calculator = Substitute.For<IProfileChangeCalculator>();
            var processor = Substitute.For<IProfileChangeProcessor>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new ProfileCommand(store, calculator, processor, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                cache.GetProfileResults().Returns(cacheResults);
                store.GetProfile(profile.Id, tokenSource.Token).Returns(profile);
                calculator.CalculateChanges(profile, expected).Returns(changeResult);

                await sut.UpdateProfile(profile.Id, expected, tokenSource.Token).ConfigureAwait(false);

                cache.Received().StoreProfileResults(
                    Verify.That<ICollection<ProfileResult>>(x => x.Should().Contain(y => y.Id == profile.Id)));
            }
        }

        [Fact]
        public async Task UpdateProfileCalculatesAndProcessesChangesForUpdatedProfileNotBannedTest()
        {
            var expected = Model.Create<UpdatableProfile>();
            var profile = Model.Create<Profile>().Set(x => x.BannedAt = null);
            var changeResult = Model.Create<ProfileChangeResult>().Set(x => x.ProfileChanged = true);

            var store = Substitute.For<IProfileStore>();
            var calculator = Substitute.For<IProfileChangeCalculator>();
            var processor = Substitute.For<IProfileChangeProcessor>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new ProfileCommand(store, calculator, processor, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                store.GetProfile(profile.Id, tokenSource.Token).Returns(profile);
                calculator.CalculateChanges(profile, expected).Returns(changeResult);

                await sut.UpdateProfile(profile.Id, expected, tokenSource.Token).ConfigureAwait(false);

                await processor.Received().Execute(
                    Verify.That<Profile>(x => x.ShouldBeEquivalentTo(expected, opt => opt.ExcludingMissingMembers())),
                    changeResult,
                    tokenSource.Token).ConfigureAwait(false);
            }
        }

        [Fact]
        public async Task UpdateProfileDoesNotAddHiddenProfileToResultsCacheWhenNotPreviouslyCachedTest()
        {
            var expected = Model.Create<UpdatableProfile>().Set(x => x.Status = ProfileStatus.Hidden);
            var profile = Model.Create<Profile>().Set(x => x.BannedAt = null);
            var changeResult = Model.Create<ProfileChangeResult>().Set(x => x.ProfileChanged = true);
            var cacheResults = new List<ProfileResult>();

            var store = Substitute.For<IProfileStore>();
            var calculator = Substitute.For<IProfileChangeCalculator>();
            var processor = Substitute.For<IProfileChangeProcessor>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new ProfileCommand(store, calculator, processor, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                cache.GetProfileResults().Returns(cacheResults);
                store.GetProfile(profile.Id, tokenSource.Token).Returns(profile);
                calculator.CalculateChanges(profile, expected).Returns(changeResult);

                await sut.UpdateProfile(profile.Id, expected, tokenSource.Token).ConfigureAwait(false);

                cache.DidNotReceive().StoreProfileResults(Arg.Any<ICollection<ProfileResult>>());
            }
        }

        [Fact]
        public async Task UpdateProfileDoesNotCacheProfileResultWhenNoResultsAreCachedTest()
        {
            var expected = Model.Create<UpdatableProfile>();
            var profile = Model.Create<Profile>().Set(x => x.BannedAt = null);
            var changeResult = Model.Create<ProfileChangeResult>().Set(x => x.ProfileChanged = true);

            var store = Substitute.For<IProfileStore>();
            var calculator = Substitute.For<IProfileChangeCalculator>();
            var processor = Substitute.For<IProfileChangeProcessor>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new ProfileCommand(store, calculator, processor, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                cache.GetProfileResults().Returns((ICollection<ProfileResult>) null);
                store.GetProfile(profile.Id, tokenSource.Token).Returns(profile);
                calculator.CalculateChanges(profile, expected).Returns(changeResult);

                await sut.UpdateProfile(profile.Id, expected, tokenSource.Token).ConfigureAwait(false);

                cache.DidNotReceive().StoreProfileResults(Arg.Any<ICollection<ProfileResult>>());
            }
        }

        [Fact]
        public async Task UpdateProfileDoesNotProcessProfileChangesWhenNoChangeFoundTest()
        {
            var expected = Model.Create<UpdatableProfile>();
            var profile = Model.Create<Profile>().Set(x => x.BannedAt = null);
            var changeResult = Model.Create<ProfileChangeResult>().Set(x => x.ProfileChanged = false);

            var store = Substitute.For<IProfileStore>();
            var calculator = Substitute.For<IProfileChangeCalculator>();
            var processor = Substitute.For<IProfileChangeProcessor>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new ProfileCommand(store, calculator, processor, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                store.GetProfile(profile.Id, tokenSource.Token).Returns(profile);
                calculator.CalculateChanges(profile, expected).Returns(changeResult);

                await sut.UpdateProfile(profile.Id, expected, tokenSource.Token).ConfigureAwait(false);

                await processor.DidNotReceive().Execute(
                    Arg.Any<Profile>(),
                    Arg.Any<ProfileChangeResult>(),
                    Arg.Any<CancellationToken>()).ConfigureAwait(false);
            }
        }

        [Fact]
        public async Task UpdateProfileRemovesHiddenProfileFromResultsCacheTest()
        {
            var expected = Model.Create<UpdatableProfile>().Set(x => x.Status = ProfileStatus.Hidden);
            var profile = Model.Create<Profile>().Set(x => x.BannedAt = null);
            var changeResult = Model.Create<ProfileChangeResult>().Set(x => x.ProfileChanged = true);
            var cacheResults = new List<ProfileResult>
            {
                Model.Create<ProfileResult>().Set(x => x.Id = profile.Id)
            };

            var store = Substitute.For<IProfileStore>();
            var calculator = Substitute.For<IProfileChangeCalculator>();
            var processor = Substitute.For<IProfileChangeProcessor>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new ProfileCommand(store, calculator, processor, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                cache.GetProfileResults().Returns(cacheResults);
                store.GetProfile(profile.Id, tokenSource.Token).Returns(profile);
                calculator.CalculateChanges(profile, expected).Returns(changeResult);

                await sut.UpdateProfile(profile.Id, expected, tokenSource.Token).ConfigureAwait(false);

                cache.Received().StoreProfileResults(
                    Verify.That<ICollection<ProfileResult>>(x => x.Should().NotContain(y => y.Id == profile.Id)));
            }
        }

        [Fact]
        public async Task UpdateProfileStoresProfileWithOriginalProfileBannedValueTest()
        {
            var expected = Model.Create<UpdatableProfile>();
            var profile = Model.Create<Profile>().Set(x => x.BannedAt = DateTimeOffset.UtcNow);

            var store = Substitute.For<IProfileStore>();
            var calculator = Substitute.For<IProfileChangeCalculator>();
            var processor = Substitute.For<IProfileChangeProcessor>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new ProfileCommand(store, calculator, processor, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                store.GetProfile(profile.Id, tokenSource.Token).Returns(profile);

                await sut.UpdateProfile(profile.Id, expected, tokenSource.Token).ConfigureAwait(false);

                await store.Received().StoreProfile(
                    Arg.Is<Profile>(x => x.BannedAt == profile.BannedAt),
                    tokenSource.Token).ConfigureAwait(false);
                cache.DidNotReceive().StoreProfile(Arg.Any<Profile>());
            }
        }

        [Fact]
        public void UpdateProfileThrowsExceptionWithEmptyProfileIdTest()
        {
            var profileId = Guid.Empty;
            var profile = Model.Create<UpdatableProfile>();

            var store = Substitute.For<IProfileStore>();
            var calculator = Substitute.For<IProfileChangeCalculator>();
            var processor = Substitute.For<IProfileChangeProcessor>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new ProfileCommand(store, calculator, processor, cache);

            Func<Task> action = async () => await sut.UpdateProfile(profileId, profile, CancellationToken.None)
                .ConfigureAwait(false);

            action.ShouldThrow<ArgumentException>();
        }

        [Fact]
        public void UpdateProfileThrowsExceptionWithNullProfileTest()
        {
            var profileId = Guid.NewGuid();
            var store = Substitute.For<IProfileStore>();
            var calculator = Substitute.For<IProfileChangeCalculator>();
            var processor = Substitute.For<IProfileChangeProcessor>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new ProfileCommand(store, calculator, processor, cache);

            Func<Task> action = async () => await sut.UpdateProfile(profileId, null, CancellationToken.None)
                .ConfigureAwait(false);

            action.ShouldThrow<ArgumentNullException>();
        }

        [Theory]
        [InlineData(ProfileStatus.Available)]
        [InlineData(ProfileStatus.Unavailable)]
        public async Task UpdateProfileUpdatesProfileInResultsCacheTest(ProfileStatus status)
        {
            var expected = Model.Create<UpdatableProfile>().Set(x => x.Status = status);
            var profile = Model.Create<Profile>().Set(x => x.BannedAt = null);
            var changeResult = Model.Create<ProfileChangeResult>().Set(x => x.ProfileChanged = true);
            var cacheResults = new List<ProfileResult>
            {
                Model.Create<ProfileResult>().Set(x => x.Id = profile.Id)
            };

            var store = Substitute.For<IProfileStore>();
            var calculator = Substitute.For<IProfileChangeCalculator>();
            var processor = Substitute.For<IProfileChangeProcessor>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new ProfileCommand(store, calculator, processor, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                cache.GetProfileResults().Returns(cacheResults);
                store.GetProfile(profile.Id, tokenSource.Token).Returns(profile);
                calculator.CalculateChanges(profile, expected).Returns(changeResult);

                await sut.UpdateProfile(profile.Id, expected, tokenSource.Token).ConfigureAwait(false);

                // This is a mouthful. 
                // We want to make sure that the profile results being put into the cache contains only a single item
                // matching our updated profile and that has all the same data as the updated profile
                cache.Received().StoreProfileResults(
                    Verify.That<ICollection<ProfileResult>>(
                        x => x.Single(y => y.Id == profile.Id)
                            .ShouldBeEquivalentTo(expected, opt => opt.ExcludingMissingMembers())));
            }
        }
    }
}