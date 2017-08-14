namespace DevMentoryApi.Business.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using DevMentorApi.Azure;
    using DevMentorApi.Business;
    using DevMentorApi.Model;
    using FluentAssertions;
    using ModelBuilder;
    using NSubstitute;
    using Xunit;

    public class ProfileSearchManagerTests
    {
        [Fact]
        public async Task GetProfileResultsReturnsAllCachedResultsWhenFiltersIsEmptyTest()
        {
            var expected = Model.Create<List<ProfileResult>>();
            var filters = new List<ProfileResultFilter>();

            var profileStore = Substitute.For<IProfileStore>();
            var linkStore = Substitute.For<ICategoryLinkStore>();
            var cache = Substitute.For<ICacheManager>();

            cache.GetProfileResults().Returns(expected);

            var sut = new ProfileSearchManager(profileStore, linkStore, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                var actual = await sut.GetProfileResults(filters, tokenSource.Token).ConfigureAwait(false);

                actual.ShouldBeEquivalentTo(expected);
            }
        }

        [Fact]
        public async Task GetProfileResultsReturnsAllCachedResultsWhenFiltersIsNullTest()
        {
            var expected = Model.Create<List<ProfileResult>>();

            var profileStore = Substitute.For<IProfileStore>();
            var linkStore = Substitute.For<ICategoryLinkStore>();
            var cache = Substitute.For<ICacheManager>();

            cache.GetProfileResults().Returns(expected);

            var sut = new ProfileSearchManager(profileStore, linkStore, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                var actual = await sut.GetProfileResults(null, tokenSource.Token).ConfigureAwait(false);

                actual.ShouldBeEquivalentTo(expected);
            }
        }

        [Fact]
        public async Task GetProfileResultsReturnsAllStoreResultsWhenFiltersIsEmptyTest()
        {
            var expected = Model.Create<List<ProfileResult>>();
            var filters = new List<ProfileResultFilter>();

            var profileStore = Substitute.For<IProfileStore>();
            var linkStore = Substitute.For<ICategoryLinkStore>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new ProfileSearchManager(profileStore, linkStore, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                cache.GetProfileResults().Returns((ICollection<ProfileResult>) null);
                profileStore.GetProfileResults(tokenSource.Token).Returns(expected);

                var actual = await sut.GetProfileResults(filters, tokenSource.Token).ConfigureAwait(false);

                actual.ShouldBeEquivalentTo(expected);
                cache.Received()
                    .StoreProfileResults(
                        Verify.That<ICollection<ProfileResult>>(x => x.ShouldBeEquivalentTo(expected)));
            }
        }

        [Fact]
        public async Task GetProfileResultsReturnsAllStoreResultsWhenFiltersIsNullTest()
        {
            var originalAutoPopulateCount = EnumerableTypeCreator.DefaultAutoPopulateCount;
            EnumerableTypeCreator.DefaultAutoPopulateCount = 100;

            List<ProfileResult> expected;

            try
            {
                expected = Model.Create<List<ProfileResult>>();
            }
            finally
            {
                EnumerableTypeCreator.DefaultAutoPopulateCount = originalAutoPopulateCount;
            }

            var profileStore = Substitute.For<IProfileStore>();
            var linkStore = Substitute.For<ICategoryLinkStore>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new ProfileSearchManager(profileStore, linkStore, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                cache.GetProfileResults().Returns((ICollection<ProfileResult>) null);
                profileStore.GetProfileResults(tokenSource.Token).Returns(expected);

                var actual = await sut.GetProfileResults(null, tokenSource.Token).ConfigureAwait(false);

                actual.ShouldBeEquivalentTo(expected);
                cache.Received()
                    .StoreProfileResults(
                        Verify.That<ICollection<ProfileResult>>(x => x.ShouldBeEquivalentTo(expected)));
            }
        }

        [Fact]
        public async Task GetProfileResultsReturnsResultsWithExpectedSortOrderTest()
        {
            var source = Model.Create<List<ProfileResult>>();
            var expected = (from x in source
                orderby x.Status descending, x.YearStartedInTech ?? 0 descending, x.BirthYear ??
                                                                                  DateTimeOffset.UtcNow.Year
                select x).ToList();

            var filters = new List<ProfileResultFilter>();

            var profileStore = Substitute.For<IProfileStore>();
            var linkStore = Substitute.For<ICategoryLinkStore>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new ProfileSearchManager(profileStore, linkStore, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                cache.GetProfileResults().Returns((ICollection<ProfileResult>) null);
                profileStore.GetProfileResults(tokenSource.Token).Returns(source);

                var actual = await sut.GetProfileResults(filters, tokenSource.Token).ConfigureAwait(false);

                actual.Should().ContainInOrder(expected);
                cache.Received()
                    .StoreProfileResults(
                        Verify.That<ICollection<ProfileResult>>(x => x.Should().ContainInOrder(expected)));
            }
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullCacheManagerTest()
        {
            var profileStore = Substitute.For<IProfileStore>();
            var linkStore = Substitute.For<ICategoryLinkStore>();

            Action action = () => new ProfileSearchManager(profileStore, linkStore, null);

            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullLinkStoreTest()
        {
            var profileStore = Substitute.For<IProfileStore>();
            var cache = Substitute.For<ICacheManager>();

            Action action = () => new ProfileSearchManager(profileStore, null, cache);

            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullProfileStoreTest()
        {
            var linkStore = Substitute.For<ICategoryLinkStore>();
            var cache = Substitute.For<ICacheManager>();

            Action action = () => new ProfileSearchManager(null, linkStore, cache);

            action.ShouldThrow<ArgumentNullException>();
        }
    }
}