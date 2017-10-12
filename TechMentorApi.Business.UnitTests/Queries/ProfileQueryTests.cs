namespace TechMentorApi.Business.UnitTests.Queries
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using ModelBuilder;
    using NSubstitute;
    using TechMentorApi.Azure;
    using TechMentorApi.Business.Queries;
    using TechMentorApi.Model;
    using Xunit;

    public class ProfileQueryTests
    {
        [Fact]
        public async Task GetProfileCachesProfileReturnedFromStoreTest()
        {
            var expected = Model.Create<Profile>().Set(x => x.Status = ProfileStatus.Unavailable);

            var profileStore = Substitute.For<IProfileStore>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new ProfileQuery(profileStore, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                profileStore.GetProfile(expected.Id, tokenSource.Token).Returns(expected);

                var actual = await sut.GetProfile(expected.Id, tokenSource.Token).ConfigureAwait(false);

                cache.Received().StoreProfile(actual);
            }
        }

        [Fact]
        public async Task GetProfileReturnsBannedProfileFromCachedTest()
        {
            var expected = Model.Create<Profile>().Set(x => x.Status = ProfileStatus.Available)
                .Set(x => x.BannedAt = DateTimeOffset.UtcNow);

            var profileStore = Substitute.For<IProfileStore>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new ProfileQuery(profileStore, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                cache.GetProfile(expected.Id).Returns(expected);

                var actual = await sut.GetProfile(expected.Id, tokenSource.Token).ConfigureAwait(false);

                actual.ShouldBeEquivalentTo(expected, opt => opt.ExcludingMissingMembers());
                cache.DidNotReceive().StoreProfile(Arg.Any<Profile>());
            }
        }

        [Fact]
        public async Task GetProfileReturnsBannedProfileFromStoreTest()
        {
            var expected = Model.Create<Profile>().Set(x => x.Status = ProfileStatus.Available)
                .Set(x => x.BannedAt = DateTimeOffset.UtcNow);

            var profileStore = Substitute.For<IProfileStore>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new ProfileQuery(profileStore, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                profileStore.GetProfile(expected.Id, tokenSource.Token).Returns(expected);

                var actual = await sut.GetProfile(expected.Id, tokenSource.Token).ConfigureAwait(false);

                actual.ShouldBeEquivalentTo(expected, opt => opt.ExcludingMissingMembers());
                cache.Received().StoreProfile(expected);
            }
        }

        [Fact]
        public async Task GetProfileReturnsCachedProfileTest()
        {
            var expected = Model.Create<Profile>().Set(x => x.Status = ProfileStatus.Unavailable);

            var profileStore = Substitute.For<IProfileStore>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new ProfileQuery(profileStore, cache);

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
            var expected = Model.Create<Profile>().Set(x => x.Status = ProfileStatus.Hidden)
                .Set(x => x.BannedAt = null);

            var profileStore = Substitute.For<IProfileStore>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new ProfileQuery(profileStore, cache);

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
            var expected = Model.Create<Profile>().Set(x => x.Status = ProfileStatus.Hidden)
                .Set(x => x.BannedAt = null);

            var profileStore = Substitute.For<IProfileStore>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new ProfileQuery(profileStore, cache);

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

            var sut = new ProfileQuery(profileStore, cache);

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
            var cache = Substitute.For<ICacheManager>();

            var sut = new ProfileQuery(store, cache);

            Func<Task> action = async () => await sut.GetProfile(Guid.Empty, CancellationToken.None)
                .ConfigureAwait(false);

            action.ShouldThrow<ArgumentException>();
        }

        [Fact]
        public async Task GetPublicProfileCachesHiddenProfileReturnedFromStoreTest()
        {
            var expected = Model.Create<Profile>().Set(x => x.Status = ProfileStatus.Hidden)
                .Set(x => x.BannedAt = null);

            var profileStore = Substitute.For<IProfileStore>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new ProfileQuery(profileStore, cache);

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
            var expected = Model.Create<Profile>().Set(x => x.Status = ProfileStatus.Unavailable)
                .Set(x => x.BannedAt = null);

            var profileStore = Substitute.For<IProfileStore>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new ProfileQuery(profileStore, cache);

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
            var expected = Model.Create<Profile>().Set(x => x.Status = ProfileStatus.Unavailable)
                .Set(x => x.BannedAt = null);

            var profileStore = Substitute.For<IProfileStore>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new ProfileQuery(profileStore, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                cache.GetProfile(expected.Id).Returns(expected);

                var actual = await sut.GetPublicProfile(expected.Id, tokenSource.Token).ConfigureAwait(false);

                actual.ShouldBeEquivalentTo(expected, opt => opt.ExcludingMissingMembers());
            }
        }

        [Fact]
        public async Task GetPublicProfileReturnsNullWhenCachedProfileIsBannedTest()
        {
            var expected = Model.Create<Profile>().Set(x => x.Status = ProfileStatus.Available)
                .Set(x => x.BannedAt = DateTimeOffset.UtcNow);

            var profileStore = Substitute.For<IProfileStore>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new ProfileQuery(profileStore, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                cache.GetProfile(expected.Id).Returns(expected);

                var actual = await sut.GetPublicProfile(expected.Id, tokenSource.Token).ConfigureAwait(false);

                actual.Should().BeNull();
                cache.DidNotReceive().StoreProfile(Arg.Any<Profile>());
            }
        }

        [Fact]
        public async Task GetPublicProfileReturnsNullWhenCachedProfileIsHiddenTest()
        {
            var expected = Model.Create<Profile>().Set(x => x.Status = ProfileStatus.Hidden)
                .Set(x => x.BannedAt = null);

            var profileStore = Substitute.For<IProfileStore>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new ProfileQuery(profileStore, cache);

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

            var sut = new ProfileQuery(profileStore, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                var actual = await sut.GetPublicProfile(expected.Id, tokenSource.Token).ConfigureAwait(false);

                actual.Should().BeNull();
                cache.DidNotReceive().StoreProfile(Arg.Any<Profile>());
            }
        }

        [Fact]
        public async Task GetPublicProfileReturnsNullWhenStoreProfileIsBannedTest()
        {
            var expected = Model.Create<Profile>().Set(x => x.BannedAt = DateTimeOffset.UtcNow);

            var profileStore = Substitute.For<IProfileStore>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new ProfileQuery(profileStore, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                profileStore.GetProfile(expected.Id, tokenSource.Token).Returns(expected);

                var actual = await sut.GetPublicProfile(expected.Id, tokenSource.Token).ConfigureAwait(false);

                actual.Should().BeNull();
                cache.Received().StoreProfile(expected);
            }
        }

        [Fact]
        public async Task GetPublicProfileReturnsNullWhenStoreProfileIsHiddenTest()
        {
            var expected = Model.Create<Profile>().Set(x => x.Status = ProfileStatus.Hidden);

            var profileStore = Substitute.For<IProfileStore>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new ProfileQuery(profileStore, cache);

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
            var cache = Substitute.For<ICacheManager>();

            var sut = new ProfileQuery(store, cache);

            Func<Task> action = async () => await sut.GetPublicProfile(Guid.Empty, CancellationToken.None)
                .ConfigureAwait(false);

            action.ShouldThrow<ArgumentException>();
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullCacheTest()
        {
            var profileStore = Substitute.For<IProfileStore>();

            Action action = () => new ProfileQuery(profileStore, null);

            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullProfileStoreTest()
        {
            var cache = Substitute.For<ICacheManager>();

            Action action = () => new ProfileQuery(null, cache);

            action.ShouldThrow<ArgumentNullException>();
        }
    }
}