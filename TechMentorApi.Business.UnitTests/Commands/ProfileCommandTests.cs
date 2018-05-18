namespace TechMentorApi.Business.UnitTests.Commands
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
    using Xunit.Abstractions;

    public class ProfileCommandTests
    {
        private readonly ITestOutputHelper _output;

        public ProfileCommandTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task BanProfileCallsStoreWithBanInformationTest()
        {
            var profile = Model.Create<Profile>();
            var bannedAt = DateTimeOffset.UtcNow.AddDays(-2);

            var store = Substitute.For<IProfileStore>();
            var calculator = Substitute.For<IProfileChangeCalculator>();
            var processor = Substitute.For<IProfileChangeProcessor>();
            var cache = Substitute.For<IProfileCache>();

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
            var cache = Substitute.For<IProfileCache>();

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
            var cache = Substitute.For<IProfileCache>();

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
            var cache = Substitute.For<IProfileCache>();

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
            var cache = Substitute.For<IProfileCache>();

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
                {Model.Create<ProfileResult>().Set(x => x.Id = profile.Id)};

            var store = Substitute.For<IProfileStore>();
            var calculator = Substitute.For<IProfileChangeCalculator>();
            var processor = Substitute.For<IProfileChangeProcessor>();
            var cache = Substitute.For<IProfileCache>();

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
            var cache = Substitute.For<IProfileCache>();

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
            var cache = Substitute.For<IProfileCache>();

            var sut = new ProfileCommand(store, calculator, processor, cache);

            Func<Task> action = async () => await sut.BanProfile(Guid.Empty, bannedAt, CancellationToken.None)
                .ConfigureAwait(false);

            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public async Task DeleteProfileDoesNotCacheProfileResultWhenNoResultsAreCachedTest()
        {
            var profile = Model.Create<Profile>();

            var store = Substitute.For<IProfileStore>();
            var calculator = Substitute.For<IProfileChangeCalculator>();
            var processor = Substitute.For<IProfileChangeProcessor>();
            var cache = Substitute.For<IProfileCache>();

            var sut = new ProfileCommand(store, calculator, processor, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                store.GetProfile(profile.Id, tokenSource.Token).Returns(profile);
                calculator.RemoveAllCategoryLinks(profile).Returns(new ProfileChangeResult());
                store.DeleteProfile(profile.Id, tokenSource.Token).Returns(profile);
                cache.GetProfileResults().Returns((ICollection<ProfileResult>) null);

                await sut.DeleteProfile(profile.Id, tokenSource.Token).ConfigureAwait(false);

                cache.DidNotReceive().StoreProfileResults(Arg.Any<ICollection<ProfileResult>>());
            }
        }

        [Fact]
        public async Task DeleteProfileDoesNotProcessChangesWhenNoneFoundTest()
        {
            var profile = Model.Create<Profile>();

            var store = Substitute.For<IProfileStore>();
            var calculator = Substitute.For<IProfileChangeCalculator>();
            var processor = Substitute.For<IProfileChangeProcessor>();
            var cache = Substitute.For<IProfileCache>();

            var sut = new ProfileCommand(store, calculator, processor, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                store.GetProfile(profile.Id, tokenSource.Token).Returns(profile);
                calculator.RemoveAllCategoryLinks(profile).Returns(new ProfileChangeResult());
                store.DeleteProfile(profile.Id, tokenSource.Token).Returns(profile);

                await sut.DeleteProfile(profile.Id, tokenSource.Token).ConfigureAwait(false);

                await processor.DidNotReceive().Execute(
                    Arg.Any<Profile>(),
                    Arg.Any<ProfileChangeResult>(),
                    Arg.Any<CancellationToken>()).ConfigureAwait(false);
            }
        }

        [Fact]
        public async Task DeleteProfileIgnoresWhenProfileNotFoundTest()
        {
            var profileId = Guid.NewGuid();

            var store = Substitute.For<IProfileStore>();
            var calculator = Substitute.For<IProfileChangeCalculator>();
            var processor = Substitute.For<IProfileChangeProcessor>();
            var cache = Substitute.For<IProfileCache>();

            var sut = new ProfileCommand(store, calculator, processor, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                await sut.DeleteProfile(profileId, tokenSource.Token).ConfigureAwait(false);
            }
        }

        [Fact]
        public async Task DeleteProfileRemovesCategoryLinksTest()
        {
            var profile = Model.Create<Profile>();
            var changeResult = Model.Create<ProfileChangeResult>();

            var store = Substitute.For<IProfileStore>();
            var calculator = Substitute.For<IProfileChangeCalculator>();
            var processor = Substitute.For<IProfileChangeProcessor>();
            var cache = Substitute.For<IProfileCache>();

            var sut = new ProfileCommand(store, calculator, processor, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                store.GetProfile(profile.Id, tokenSource.Token).Returns(profile);
                calculator.RemoveAllCategoryLinks(profile).Returns(changeResult);
                store.DeleteProfile(profile.Id, tokenSource.Token).Returns(profile);

                await sut.DeleteProfile(profile.Id, tokenSource.Token).ConfigureAwait(false);

                await processor.Received().Execute(profile, changeResult, tokenSource.Token).ConfigureAwait(false);
            }
        }

        [Fact]
        public async Task DeleteProfileRemovesProfileFromCacheTest()
        {
            var profile = Model.Create<Profile>();

            var store = Substitute.For<IProfileStore>();
            var calculator = Substitute.For<IProfileChangeCalculator>();
            var processor = Substitute.For<IProfileChangeProcessor>();
            var cache = Substitute.For<IProfileCache>();

            var sut = new ProfileCommand(store, calculator, processor, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                store.GetProfile(profile.Id, tokenSource.Token).Returns(profile);
                calculator.RemoveAllCategoryLinks(profile).Returns(new ProfileChangeResult());
                store.DeleteProfile(profile.Id, tokenSource.Token).Returns(profile);

                await sut.DeleteProfile(profile.Id, tokenSource.Token).ConfigureAwait(false);

                cache.Received().RemoveProfile(profile.Id);
                cache.DidNotReceive().StoreProfile(Arg.Any<Profile>());
            }
        }

        [Fact]
        public async Task DeleteProfileRemovesProfileFromResultsCacheTest()
        {
            var profile = Model.Create<Profile>();
            var cacheResults = new List<ProfileResult>
                {Model.Create<ProfileResult>().Set(x => x.Id = profile.Id)};

            var store = Substitute.For<IProfileStore>();
            var calculator = Substitute.For<IProfileChangeCalculator>();
            var processor = Substitute.For<IProfileChangeProcessor>();
            var cache = Substitute.For<IProfileCache>();

            var sut = new ProfileCommand(store, calculator, processor, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                store.GetProfile(profile.Id, tokenSource.Token).Returns(profile);
                calculator.RemoveAllCategoryLinks(profile).Returns(new ProfileChangeResult());
                store.DeleteProfile(profile.Id, tokenSource.Token).Returns(profile);
                cache.GetProfileResults().Returns(cacheResults);

                await sut.DeleteProfile(profile.Id, tokenSource.Token).ConfigureAwait(false);

                cache.Received().StoreProfileResults(
                    Verify.That<ICollection<ProfileResult>>(x => x.Should().NotContain(y => y.Id == profile.Id)));
            }
        }

        [Fact]
        public async Task DeleteProfileRemovesProfileFromStoreTest()
        {
            var profile = Model.Create<Profile>();

            var store = Substitute.For<IProfileStore>();
            var calculator = Substitute.For<IProfileChangeCalculator>();
            var processor = Substitute.For<IProfileChangeProcessor>();
            var cache = Substitute.For<IProfileCache>();

            var sut = new ProfileCommand(store, calculator, processor, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                store.GetProfile(profile.Id, tokenSource.Token).Returns(profile);
                calculator.RemoveAllCategoryLinks(profile).Returns(new ProfileChangeResult());
                store.DeleteProfile(profile.Id, tokenSource.Token).Returns(profile);

                await sut.DeleteProfile(profile.Id, tokenSource.Token).ConfigureAwait(false);

                await store.Received().DeleteProfile(profile.Id, tokenSource.Token).ConfigureAwait(false);
            }
        }

        [Fact]
        public void DeleteProfileThrowsExceptionWithEmptyIdTest()
        {
            var store = Substitute.For<IProfileStore>();
            var calculator = Substitute.For<IProfileChangeCalculator>();
            var processor = Substitute.For<IProfileChangeProcessor>();
            var cache = Substitute.For<IProfileCache>();

            var sut = new ProfileCommand(store, calculator, processor, cache);

            Func<Task> action = async () => await sut.DeleteProfile(Guid.Empty, CancellationToken.None)
                .ConfigureAwait(false);

            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullCacheTest()
        {
            var profileStore = Substitute.For<IProfileStore>();
            var calculator = Substitute.For<IProfileChangeCalculator>();
            var processor = Substitute.For<IProfileChangeProcessor>();

            Action action = () => new ProfileCommand(profileStore, calculator, processor, null);

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullCalculatorTest()
        {
            var profileStore = Substitute.For<IProfileStore>();
            var processor = Substitute.For<IProfileChangeProcessor>();
            var cache = Substitute.For<IProfileCache>();

            Action action = () => new ProfileCommand(profileStore, null, processor, cache);

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullProcessorTest()
        {
            var profileStore = Substitute.For<IProfileStore>();
            var calculator = Substitute.For<IProfileChangeCalculator>();
            var cache = Substitute.For<IProfileCache>();

            Action action = () => new ProfileCommand(profileStore, calculator, null, cache);

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullProfileStoreTest()
        {
            var cache = Substitute.For<IProfileCache>();
            var calculator = Substitute.For<IProfileChangeCalculator>();
            var processor = Substitute.For<IProfileChangeProcessor>();

            Action action = () => new ProfileCommand(null, calculator, processor, cache);

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public async Task UpdateProfileAddsProfileToResultsCacheWhenNotPreviouslyCachedTest()
        {
            var expected = Model.Create<UpdatableProfile>().Set(x => x.Status = ProfileStatus.Available);
            var profile = Model.Create<Profile>().Set(x => x.BannedAt = null)
                .Set(x => x.Status = ProfileStatus.Available);
            var changeResult = Model.Create<ProfileChangeResult>().Set(x => x.ProfileChanged = true);
            var cacheResults = new List<ProfileResult>();

            var store = Substitute.For<IProfileStore>();
            var calculator = Substitute.For<IProfileChangeCalculator>();
            var processor = Substitute.For<IProfileChangeProcessor>();
            var cache = Substitute.For<IProfileCache>();

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
        public async Task UpdateProfileCalculatesAndProcessesChangesForUpdatedProfileTest()
        {
            var expected = Model.Create<UpdatableProfile>();
            var profile = Model.Create<Profile>().Set(x =>
            {
                x.AcceptCoC = true;
                x.AcceptToS = true;
            });
            var changeResult = Model.Create<ProfileChangeResult>().Set(x => x.ProfileChanged = true);

            var store = Substitute.For<IProfileStore>();
            var calculator = Substitute.For<IProfileChangeCalculator>();
            var processor = Substitute.For<IProfileChangeProcessor>();
            var cache = Substitute.For<IProfileCache>();

            var sut = new ProfileCommand(store, calculator, processor, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                store.GetProfile(profile.Id, tokenSource.Token).Returns(profile);
                calculator.CalculateChanges(profile, expected).Returns(changeResult);

                await sut.UpdateProfile(profile.Id, expected, tokenSource.Token).ConfigureAwait(false);

                await processor.Received().Execute(
                    Verify.That<Profile>(x =>
                        x.Should().BeEquivalentTo(expected, opt => opt.ExcludingMissingMembers())),
                    changeResult,
                    tokenSource.Token).ConfigureAwait(false);
            }
        }

        [Fact]
        public async Task UpdateProfileDoesNotAddBannedProfileToResultsCacheWhenNotPreviouslyCachedTest()
        {
            var expected = Model.Create<UpdatableProfile>().Set(x => x.Status = ProfileStatus.Available);
            var profile = Model.Create<Profile>().Set(x => x.BannedAt = DateTimeOffset.UtcNow);
            var changeResult = Model.Create<ProfileChangeResult>().Set(x => x.ProfileChanged = true);
            var cacheResults = new List<ProfileResult>();

            var store = Substitute.For<IProfileStore>();
            var calculator = Substitute.For<IProfileChangeCalculator>();
            var processor = Substitute.For<IProfileChangeProcessor>();
            var cache = Substitute.For<IProfileCache>();

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
        public async Task UpdateProfileDoesNotAddHiddenProfileToResultsCacheWhenNotPreviouslyCachedTest()
        {
            var expected = Model.Create<UpdatableProfile>().Set(x => x.Status = ProfileStatus.Hidden);
            var profile = Model.Create<Profile>().Set(x => x.BannedAt = null);
            var changeResult = Model.Create<ProfileChangeResult>().Set(x => x.ProfileChanged = true);
            var cacheResults = new List<ProfileResult>();

            var store = Substitute.For<IProfileStore>();
            var calculator = Substitute.For<IProfileChangeCalculator>();
            var processor = Substitute.For<IProfileChangeProcessor>();
            var cache = Substitute.For<IProfileCache>();

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
            var cache = Substitute.For<IProfileCache>();

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
            var profile = Model.Create<Profile>();
            var changeResult = Model.Create<ProfileChangeResult>().Set(x => x.ProfileChanged = false);

            var store = Substitute.For<IProfileStore>();
            var calculator = Substitute.For<IProfileChangeCalculator>();
            var processor = Substitute.For<IProfileChangeProcessor>();
            var cache = Substitute.For<IProfileCache>();

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
        public async Task UpdateProfileRemovesBannedProfileFromResultsCacheTest()
        {
            var expected = Model.Create<UpdatableProfile>().Set(x => x.Status = ProfileStatus.Available);
            var profile = Model.Create<Profile>().Set(x => x.BannedAt = DateTimeOffset.UtcNow);
            var changeResult = Model.Create<ProfileChangeResult>().Set(x => x.ProfileChanged = true);
            var cacheResults = new List<ProfileResult>
                {Model.Create<ProfileResult>().Set(x => x.Id = profile.Id)};

            var store = Substitute.For<IProfileStore>();
            var calculator = Substitute.For<IProfileChangeCalculator>();
            var processor = Substitute.For<IProfileChangeProcessor>();
            var cache = Substitute.For<IProfileCache>();

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
        public async Task UpdateProfileRemovesHiddenProfileFromResultsCacheTest()
        {
            var expected = Model.Create<UpdatableProfile>().Set(x => x.Status = ProfileStatus.Hidden);
            var profile = Model.Create<Profile>().Set(x => x.BannedAt = null);
            var changeResult = Model.Create<ProfileChangeResult>().Set(x => x.ProfileChanged = true);
            var cacheResults = new List<ProfileResult>
                {Model.Create<ProfileResult>().Set(x => x.Id = profile.Id)};

            var store = Substitute.For<IProfileStore>();
            var calculator = Substitute.For<IProfileChangeCalculator>();
            var processor = Substitute.For<IProfileChangeProcessor>();
            var cache = Substitute.For<IProfileCache>();

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

        [Theory]
        [InlineData(true, true, false)]
        [InlineData(true, false, false)]
        [InlineData(false, true, true)]
        [InlineData(false, false, false)]
        public async Task UpdateProfileStoresProfileWithAcceptedCoCAtDependingOnConsentTest(bool originalConsent,
            bool updatedConsent, bool timeSet)
        {
            var expected = Model.Create<UpdatableProfile>().Set(x => x.AcceptCoC = updatedConsent);
            var profile = Model.Create<Profile>().Set(x =>
            {
                x.AcceptCoC = originalConsent;
                x.AcceptedCoCAt = DateTimeOffset.UtcNow.AddYears(-1);
            });
            var changeResult = Model.Create<ProfileChangeResult>().Set(x => x.ProfileChanged = true);

            var store = Substitute.For<IProfileStore>();
            var calculator = Substitute.For<IProfileChangeCalculator>();
            var processor = Substitute.For<IProfileChangeProcessor>();
            var cache = Substitute.For<IProfileCache>();

            var sut = new ProfileCommand(store, calculator, processor, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                store.GetProfile(profile.Id, tokenSource.Token).Returns(profile);
                calculator.CalculateChanges(profile, expected).Returns(changeResult);

                await sut.UpdateProfile(profile.Id, expected, tokenSource.Token).ConfigureAwait(false);

                if (timeSet)
                {
                    await processor.Received().Execute(
                        Verify.That<Profile>(x => x.AcceptedCoCAt.Should().BeCloseTo(DateTimeOffset.UtcNow)),
                        Arg.Any<ProfileChangeResult>(),
                        tokenSource.Token).ConfigureAwait(false);
                }
                else
                {
                    await processor.Received().Execute(
                        Arg.Is<Profile>(x => x.AcceptedCoCAt == profile.AcceptedCoCAt),
                        Arg.Any<ProfileChangeResult>(),
                        tokenSource.Token).ConfigureAwait(false);
                }
            }
        }

        [Theory]
        [InlineData(true, true, false)]
        [InlineData(true, false, false)]
        [InlineData(false, true, true)]
        [InlineData(false, false, false)]
        public async Task UpdateProfileStoresProfileWithAcceptedToSAtDependingOnConsentTest(bool originalConsent,
            bool updatedConsent, bool timeSet)
        {
            var expected = Model.Create<UpdatableProfile>().Set(x => x.AcceptToS = updatedConsent);
            var profile = Model.Create<Profile>().Set(x =>
            {
                x.AcceptToS = originalConsent;
                x.AcceptedToSAt = DateTimeOffset.UtcNow.AddYears(-1);
            });
            var changeResult = Model.Create<ProfileChangeResult>().Set(x => x.ProfileChanged = true);

            var store = Substitute.For<IProfileStore>();
            var calculator = Substitute.For<IProfileChangeCalculator>();
            var processor = Substitute.For<IProfileChangeProcessor>();
            var cache = Substitute.For<IProfileCache>();

            var sut = new ProfileCommand(store, calculator, processor, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                store.GetProfile(profile.Id, tokenSource.Token).Returns(profile);
                calculator.CalculateChanges(profile, expected).Returns(changeResult);

                await sut.UpdateProfile(profile.Id, expected, tokenSource.Token).ConfigureAwait(false);

                if (timeSet)
                {
                    await processor.Received().Execute(
                        Verify.That<Profile>(x => x.AcceptedToSAt.Should().BeCloseTo(DateTimeOffset.UtcNow, 1000)),
                        Arg.Any<ProfileChangeResult>(),
                        tokenSource.Token).ConfigureAwait(false);
                }
                else
                {
                    await processor.Received().Execute(
                        Arg.Is<Profile>(x => x.AcceptedToSAt == profile.AcceptedToSAt),
                        Arg.Any<ProfileChangeResult>(),
                        tokenSource.Token).ConfigureAwait(false);
                }
            }
        }

        [Fact]
        public async Task UpdateProfileStoresProfileWithOriginalProfileAcceptedCoCAtValueTest()
        {
            var expected = Model.Create<UpdatableProfile>();
            var profile = Model.Create<Profile>().Set(x =>
            {
                x.AcceptCoC = true;
                x.AcceptToS = true;
                x.AcceptedCoCAt = DateTimeOffset.UtcNow;
            });
            var changeResult = Model.Create<ProfileChangeResult>().Set(x => x.ProfileChanged = true);

            var store = Substitute.For<IProfileStore>();
            var calculator = Substitute.For<IProfileChangeCalculator>();
            var processor = Substitute.For<IProfileChangeProcessor>();
            var cache = Substitute.For<IProfileCache>();

            var sut = new ProfileCommand(store, calculator, processor, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                store.GetProfile(profile.Id, tokenSource.Token).Returns(profile);
                calculator.CalculateChanges(profile, expected).Returns(changeResult);

                await sut.UpdateProfile(profile.Id, expected, tokenSource.Token).ConfigureAwait(false);

                await processor.Received().Execute(
                    Arg.Is<Profile>(x => x.AcceptedCoCAt == profile.AcceptedCoCAt),
                    Arg.Any<ProfileChangeResult>(),
                    tokenSource.Token).ConfigureAwait(false);
                cache.DidNotReceive().StoreProfile(Arg.Any<Profile>());
            }
        }

        [Fact]
        public async Task UpdateProfileStoresProfileWithOriginalProfileAcceptedToSAtValueTest()
        {
            var expected = Model.Create<UpdatableProfile>();
            var profile = Model.Create<Profile>().Set(x =>
            {
                x.AcceptCoC = true;
                x.AcceptToS = true;
                x.AcceptedToSAt = DateTimeOffset.UtcNow;
            });
            var changeResult = Model.Create<ProfileChangeResult>().Set(x => x.ProfileChanged = true);

            var store = Substitute.For<IProfileStore>();
            var calculator = Substitute.For<IProfileChangeCalculator>();
            var processor = Substitute.For<IProfileChangeProcessor>();
            var cache = Substitute.For<IProfileCache>();

            var sut = new ProfileCommand(store, calculator, processor, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                store.GetProfile(profile.Id, tokenSource.Token).Returns(profile);
                calculator.CalculateChanges(profile, expected).Returns(changeResult);

                await sut.UpdateProfile(profile.Id, expected, tokenSource.Token).ConfigureAwait(false);

                if (profile.AcceptedToSAt.HasValue)
                {
                    _output.WriteLine(profile.AcceptedToSAt.Value.ToString());
                }
                else
                {
                    _output.WriteLine("No value");
                }

                await processor.Received().Execute(
                    Arg.Is<Profile>(x => x.AcceptedToSAt == profile.AcceptedToSAt),
                    Arg.Any<ProfileChangeResult>(),
                    tokenSource.Token).ConfigureAwait(false);
                cache.DidNotReceive().StoreProfile(Arg.Any<Profile>());
            }
        }

        [Fact]
        public async Task UpdateProfileStoresProfileWithOriginalProfileBannedAtValueTest()
        {
            var expected = Model.Create<UpdatableProfile>();
            var profile = Model.Create<Profile>().Set(x => x.BannedAt = DateTimeOffset.UtcNow);
            var changeResult = Model.Create<ProfileChangeResult>().Set(x => x.ProfileChanged = true);

            var store = Substitute.For<IProfileStore>();
            var calculator = Substitute.For<IProfileChangeCalculator>();
            var processor = Substitute.For<IProfileChangeProcessor>();
            var cache = Substitute.For<IProfileCache>();

            var sut = new ProfileCommand(store, calculator, processor, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                store.GetProfile(profile.Id, tokenSource.Token).Returns(profile);
                calculator.CalculateChanges(profile, expected).Returns(changeResult);

                await sut.UpdateProfile(profile.Id, expected, tokenSource.Token).ConfigureAwait(false);

                await processor.Received().Execute(
                    Arg.Is<Profile>(x => x.BannedAt == profile.BannedAt),
                    Arg.Any<ProfileChangeResult>(),
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
            var cache = Substitute.For<IProfileCache>();

            var sut = new ProfileCommand(store, calculator, processor, cache);

            Func<Task> action = async () => await sut.UpdateProfile(profileId, profile, CancellationToken.None)
                .ConfigureAwait(false);

            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void UpdateProfileThrowsExceptionWithNullProfileTest()
        {
            var profileId = Guid.NewGuid();
            var store = Substitute.For<IProfileStore>();
            var calculator = Substitute.For<IProfileChangeCalculator>();
            var processor = Substitute.For<IProfileChangeProcessor>();
            var cache = Substitute.For<IProfileCache>();

            var sut = new ProfileCommand(store, calculator, processor, cache);

            Func<Task> action = async () => await sut.UpdateProfile(profileId, null, CancellationToken.None)
                .ConfigureAwait(false);

            action.Should().Throw<ArgumentNullException>();
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
                {Model.Create<ProfileResult>().Set(x => x.Id = profile.Id)};

            var store = Substitute.For<IProfileStore>();
            var calculator = Substitute.For<IProfileChangeCalculator>();
            var processor = Substitute.For<IProfileChangeProcessor>();
            var cache = Substitute.For<IProfileCache>();

            var sut = new ProfileCommand(store, calculator, processor, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                cache.GetProfileResults().Returns(cacheResults);
                store.GetProfile(profile.Id, tokenSource.Token).Returns(profile);
                calculator.CalculateChanges(profile, expected).Returns(changeResult);

                await sut.UpdateProfile(profile.Id, expected, tokenSource.Token).ConfigureAwait(false);

                // This is a mouthful. We want to make sure that the profile results being put into
                // the cache contains only a single item matching our updated profile and that has
                // all the same data as the updated profile
                cache.Received().StoreProfileResults(
                    Verify.That<ICollection<ProfileResult>>(
                        x => x.Single(y => y.Id == profile.Id)
                            .Should().BeEquivalentTo(expected, opt => opt.ExcludingMissingMembers())));
            }
        }
    }
}