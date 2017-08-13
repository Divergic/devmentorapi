namespace DevMentoryApi.Business.UnitTests
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using DevMentorApi.Azure;
    using DevMentorApi.Business;
    using DevMentorApi.Model;
    using FluentAssertions;
    using ModelBuilder;
    using NSubstitute;
    using Xunit;

    public class ProfileManagerTests
    {
        [Fact]
        public async Task BanProfileCallsStoreWithBanInformationTest()
        {
            var profileId = Guid.NewGuid();
            var bannedAt = DateTimeOffset.UtcNow.AddDays(-2);

            var store = Substitute.For<IProfileStore>();
            var calculator = Substitute.For<IProfileChangeCalculator>();
            var processor = Substitute.For<IProfileChangeProcessor>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new ProfileManager(store, calculator, processor, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                await sut.BanProfile(profileId, bannedAt, tokenSource.Token).ConfigureAwait(false);

                await store.Received().BanProfile(profileId, bannedAt, tokenSource.Token).ConfigureAwait(false);
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

            var sut = new ProfileManager(store, calculator, processor, cache);

            Func<Task> action = async () => await sut.BanProfile(Guid.Empty, bannedAt, CancellationToken.None)
                .ConfigureAwait(false);

            action.ShouldThrow<ArgumentException>();
        }

        [Fact]
        public async Task BanProfileUpdatesCacheTest()
        {
            var expected = Model.Create<Profile>().Set(x => x.Status = ProfileStatus.Unavailable);
            var bannedAt = DateTimeOffset.UtcNow.AddDays(-2);

            var store = Substitute.For<IProfileStore>();
            var calculator = Substitute.For<IProfileChangeCalculator>();
            var processor = Substitute.For<IProfileChangeProcessor>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new ProfileManager(store, calculator, processor, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                store.BanProfile(expected.Id, bannedAt, tokenSource.Token).Returns(expected);

                await sut.BanProfile(expected.Id, bannedAt, tokenSource.Token).ConfigureAwait(false);

                cache.Received().StoreProfile(expected);
            }
        }

        [Fact]
        public async Task GetProfileCachesProfileReturnedFromStoreTest()
        {
            var expected = Model.Create<Profile>().Set(x => x.Status = ProfileStatus.Unavailable);

            var profileStore = Substitute.For<IProfileStore>();
            var cache = Substitute.For<ICacheManager>();
            var calculator = Substitute.For<IProfileChangeCalculator>();
            var processor = Substitute.For<IProfileChangeProcessor>();

            var sut = new ProfileManager(profileStore, calculator, processor, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                profileStore.GetProfile(expected.Id, tokenSource.Token).Returns(expected);

                var actual = await sut.GetProfile(expected.Id, tokenSource.Token).ConfigureAwait(false);

                cache.Received().StoreProfile(actual);
            }
        }

        [Fact]
        public async Task GetProfileReturnsCachedProfileTest()
        {
            var expected = Model.Create<Profile>().Set(x => x.Status = ProfileStatus.Unavailable);

            var profileStore = Substitute.For<IProfileStore>();
            var cache = Substitute.For<ICacheManager>();
            var calculator = Substitute.For<IProfileChangeCalculator>();
            var processor = Substitute.For<IProfileChangeProcessor>();

            var sut = new ProfileManager(profileStore, calculator, processor, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                cache.GetProfile(expected.Id).Returns(expected);

                var actual = await sut.GetProfile(expected.Id, tokenSource.Token).ConfigureAwait(false);

                actual.ShouldBeEquivalentTo(expected);
            }
        }

        [Fact]
        public async Task GetProfileReturnsHiddenProfileFromCachedTest()
        {
            var expected = Model.Create<Profile>().Set(x => x.Status = ProfileStatus.Hidden);

            var profileStore = Substitute.For<IProfileStore>();
            var cache = Substitute.For<ICacheManager>();
            var calculator = Substitute.For<IProfileChangeCalculator>();
            var processor = Substitute.For<IProfileChangeProcessor>();

            var sut = new ProfileManager(profileStore, calculator, processor, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                cache.GetProfile(expected.Id).Returns(expected);

                var actual = await sut.GetProfile(expected.Id, tokenSource.Token).ConfigureAwait(false);

                actual.ShouldBeEquivalentTo(expected, opt => opt.ExcludingMissingMembers());
                cache.DidNotReceive().StoreProfile(Arg.Any<Profile>());
            }
        }

        [Fact]
        public async Task GetProfileReturnsHiddenProfileFromStoreTest()
        {
            var expected = Model.Create<Profile>().Set(x => x.Status = ProfileStatus.Hidden);

            var profileStore = Substitute.For<IProfileStore>();
            var cache = Substitute.For<ICacheManager>();
            var calculator = Substitute.For<IProfileChangeCalculator>();
            var processor = Substitute.For<IProfileChangeProcessor>();

            var sut = new ProfileManager(profileStore, calculator, processor, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                profileStore.GetProfile(expected.Id, tokenSource.Token).Returns(expected);

                var actual = await sut.GetProfile(expected.Id, tokenSource.Token).ConfigureAwait(false);

                actual.ShouldBeEquivalentTo(expected, opt => opt.ExcludingMissingMembers());
                cache.Received().StoreProfile(expected);
            }
        }

        [Fact]
        public async Task GetProfileReturnsNullWhenProfileNotFoundTest()
        {
            var expected = Model.Create<Profile>().Set(x => x.Status = ProfileStatus.Unavailable);

            var profileStore = Substitute.For<IProfileStore>();
            var cache = Substitute.For<ICacheManager>();
            var calculator = Substitute.For<IProfileChangeCalculator>();
            var processor = Substitute.For<IProfileChangeProcessor>();

            var sut = new ProfileManager(profileStore, calculator, processor, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                var actual = await sut.GetProfile(expected.Id, tokenSource.Token).ConfigureAwait(false);

                actual.Should().BeNull();
                cache.DidNotReceive().StoreProfile(Arg.Any<Profile>());
            }
        }

        [Fact]
        public void GetProfileThrowsExceptionWithEmptyIdTest()
        {
            var store = Substitute.For<IProfileStore>();
            var calculator = Substitute.For<IProfileChangeCalculator>();
            var processor = Substitute.For<IProfileChangeProcessor>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new ProfileManager(store, calculator, processor, cache);

            Func<Task> action = async () => await sut.GetProfile(Guid.Empty, CancellationToken.None)
                .ConfigureAwait(false);

            action.ShouldThrow<ArgumentException>();
        }

        [Fact]
        public async Task GetPublicProfileCachesHiddenProfileReturnedFromStoreTest()
        {
            var expected = Model.Create<Profile>().Set(x => x.Status = ProfileStatus.Hidden);

            var profileStore = Substitute.For<IProfileStore>();
            var cache = Substitute.For<ICacheManager>();
            var calculator = Substitute.For<IProfileChangeCalculator>();
            var processor = Substitute.For<IProfileChangeProcessor>();

            var sut = new ProfileManager(profileStore, calculator, processor, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                profileStore.GetProfile(expected.Id, tokenSource.Token).Returns(expected);

                var actual = await sut.GetPublicProfile(expected.Id, tokenSource.Token).ConfigureAwait(false);

                actual.Should().BeNull();
                cache.Received().StoreProfile(expected);
            }
        }


        [Fact]
        public async Task GetPublicProfileCachesProfileReturnedFromStoreTest()
        {
            var expected = Model.Create<Profile>().Set(x => x.Status = ProfileStatus.Unavailable);

            var profileStore = Substitute.For<IProfileStore>();
            var cache = Substitute.For<ICacheManager>();
            var calculator = Substitute.For<IProfileChangeCalculator>();
            var processor = Substitute.For<IProfileChangeProcessor>();

            var sut = new ProfileManager(profileStore, calculator, processor, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                profileStore.GetProfile(expected.Id, tokenSource.Token).Returns(expected);

                var actual = await sut.GetPublicProfile(expected.Id, tokenSource.Token).ConfigureAwait(false);

                actual.ShouldBeEquivalentTo(expected, opt => opt.ExcludingMissingMembers());
                cache.Received().StoreProfile(expected);
            }
        }

        [Fact]
        public async Task GetPublicProfileReturnsCachedProfileTest()
        {
            var expected = Model.Create<Profile>().Set(x => x.Status = ProfileStatus.Unavailable);

            var profileStore = Substitute.For<IProfileStore>();
            var cache = Substitute.For<ICacheManager>();
            var calculator = Substitute.For<IProfileChangeCalculator>();
            var processor = Substitute.For<IProfileChangeProcessor>();

            var sut = new ProfileManager(profileStore, calculator, processor, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                cache.GetProfile(expected.Id).Returns(expected);

                var actual = await sut.GetPublicProfile(expected.Id, tokenSource.Token).ConfigureAwait(false);

                actual.ShouldBeEquivalentTo(expected, opt => opt.ExcludingMissingMembers());
            }
        }

        [Fact]
        public async Task GetPublicProfileReturnsNullWhenCachedProfileIsHiddenTest()
        {
            var expected = Model.Create<Profile>().Set(x => x.Status = ProfileStatus.Hidden);

            var profileStore = Substitute.For<IProfileStore>();
            var cache = Substitute.For<ICacheManager>();
            var calculator = Substitute.For<IProfileChangeCalculator>();
            var processor = Substitute.For<IProfileChangeProcessor>();

            var sut = new ProfileManager(profileStore, calculator, processor, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                cache.GetProfile(expected.Id).Returns(expected);

                var actual = await sut.GetPublicProfile(expected.Id, tokenSource.Token).ConfigureAwait(false);

                actual.Should().BeNull();
                cache.DidNotReceive().StoreProfile(Arg.Any<Profile>());
            }
        }

        [Fact]
        public async Task GetPublicProfileReturnsNullWhenProfileNotFoundTest()
        {
            var expected = Model.Create<Profile>().Set(x => x.Status = ProfileStatus.Unavailable);

            var profileStore = Substitute.For<IProfileStore>();
            var cache = Substitute.For<ICacheManager>();
            var calculator = Substitute.For<IProfileChangeCalculator>();
            var processor = Substitute.For<IProfileChangeProcessor>();

            var sut = new ProfileManager(profileStore, calculator, processor, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                var actual = await sut.GetPublicProfile(expected.Id, tokenSource.Token).ConfigureAwait(false);

                actual.Should().BeNull();
                cache.DidNotReceive().StoreProfile(Arg.Any<Profile>());
            }
        }

        [Fact]
        public async Task GetPublicProfileReturnsNullWhenStoreProfileIsHiddenTest()
        {
            var expected = Model.Create<Profile>().Set(x => x.Status = ProfileStatus.Hidden);

            var profileStore = Substitute.For<IProfileStore>();
            var cache = Substitute.For<ICacheManager>();
            var calculator = Substitute.For<IProfileChangeCalculator>();
            var processor = Substitute.For<IProfileChangeProcessor>();

            var sut = new ProfileManager(profileStore, calculator, processor, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                profileStore.GetProfile(expected.Id, tokenSource.Token).Returns(expected);

                var actual = await sut.GetPublicProfile(expected.Id, tokenSource.Token).ConfigureAwait(false);

                actual.Should().BeNull();
                cache.Received().StoreProfile(expected);
            }
        }

        [Fact]
        public void GetPublicProfileThrowsExceptionWithEmptyIdTest()
        {
            var store = Substitute.For<IProfileStore>();
            var calculator = Substitute.For<IProfileChangeCalculator>();
            var processor = Substitute.For<IProfileChangeProcessor>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new ProfileManager(store, calculator, processor, cache);

            Func<Task> action = async () => await sut.GetPublicProfile(Guid.Empty, CancellationToken.None)
                .ConfigureAwait(false);

            action.ShouldThrow<ArgumentException>();
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullCacheTest()
        {
            var profileStore = Substitute.For<IProfileStore>();
            var calculator = Substitute.For<IProfileChangeCalculator>();
            var processor = Substitute.For<IProfileChangeProcessor>();

            Action action = () => new ProfileManager(profileStore, calculator, processor, null);

            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullCalculatorTest()
        {
            var profileStore = Substitute.For<IProfileStore>();
            var processor = Substitute.For<IProfileChangeProcessor>();
            var cache = Substitute.For<ICacheManager>();

            Action action = () => new ProfileManager(profileStore, null, processor, cache);

            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullProcessorTest()
        {
            var profileStore = Substitute.For<IProfileStore>();
            var calculator = Substitute.For<IProfileChangeCalculator>();
            var cache = Substitute.For<ICacheManager>();

            Action action = () => new ProfileManager(profileStore, calculator, null, cache);

            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullProfileStoreTest()
        {
            var cache = Substitute.For<ICacheManager>();
            var calculator = Substitute.For<IProfileChangeCalculator>();
            var processor = Substitute.For<IProfileChangeProcessor>();

            Action action = () => new ProfileManager(null, calculator, processor, cache);

            action.ShouldThrow<ArgumentNullException>();
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

            var sut = new ProfileManager(store, calculator, processor, cache);

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
        public async Task UpdateProfileDoesNotProcessProfileChangesWhenNoChangeFoundTest()
        {
            var expected = Model.Create<UpdatableProfile>();
            var profile = Model.Create<Profile>().Set(x => x.BannedAt = null);
            var changeResult = Model.Create<ProfileChangeResult>().Set(x => x.ProfileChanged = false);

            var store = Substitute.For<IProfileStore>();
            var calculator = Substitute.For<IProfileChangeCalculator>();
            var processor = Substitute.For<IProfileChangeProcessor>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new ProfileManager(store, calculator, processor, cache);

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
        public async Task UpdateProfileStoresProfileWithOriginalProfileBannedValueTest()
        {
            var expected = Model.Create<UpdatableProfile>();
            var profile = Model.Create<Profile>().Set(x => x.BannedAt = DateTimeOffset.UtcNow);

            var store = Substitute.For<IProfileStore>();
            var calculator = Substitute.For<IProfileChangeCalculator>();
            var processor = Substitute.For<IProfileChangeProcessor>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new ProfileManager(store, calculator, processor, cache);

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

            var sut = new ProfileManager(store, calculator, processor, cache);

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

            var sut = new ProfileManager(store, calculator, processor, cache);

            Func<Task> action = async () => await sut.UpdateProfile(profileId, null, CancellationToken.None)
                .ConfigureAwait(false);

            action.ShouldThrow<ArgumentNullException>();
        }
    }
}