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
        public async Task GetProfileResultsCachesCategoryLinksTest()
        {
            var expected = Model.Create<List<ProfileResult>>();
            var filters = new List<ProfileFilter>
            {
                Model.Create<ProfileFilter>()
            };
            var categoryLinks = Model.Create<List<CategoryLink>>();

            var profileStore = Substitute.For<IProfileStore>();
            var linkStore = Substitute.For<ICategoryLinkStore>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new ProfileSearchManager(profileStore, linkStore, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                cache.GetProfileResults().Returns(expected);
                cache.GetCategoryLinks(Arg.Any<ProfileFilter>()).Returns((ICollection<Guid>) null);
                linkStore.GetCategoryLinks(filters[0].CategoryGroup, filters[0].CategoryName, tokenSource.Token)
                    .Returns(categoryLinks);

                await sut.GetProfileResults(filters, tokenSource.Token).ConfigureAwait(false);

                cache.Received().StoreCategoryLinks(filters[0],
                    Verify.That<ICollection<Guid>>(
                        x => x.ShouldBeEquivalentTo(categoryLinks.Select(y => y.ProfileId))));
            }
        }

        [Fact]
        public async Task GetProfileResultsDoesNotReturnProfilesMatchingOnlySomeFiltersTest()
        {
            var expected = Model.Create<List<ProfileResult>>();
            var filters = new List<ProfileFilter>
            {
                Model.Create<ProfileFilter>(),
                Model.Create<ProfileFilter>()
            };
            var firstCategoryLinks = Model.Create<List<CategoryLink>>().SetEach(x =>
            {
                x.CategoryGroup = filters[0].CategoryGroup;
                x.CategoryName = filters[0].CategoryName;
            });
            var secondCategoryLinks = Model.Create<List<CategoryLink>>().SetEach(x =>
            {
                x.CategoryGroup = filters[1].CategoryGroup;
                x.CategoryName = filters[1].CategoryName;
            });

            firstCategoryLinks[7].ProfileId = expected[3].Id;
            firstCategoryLinks[3].ProfileId = expected[5].Id;
            secondCategoryLinks[2].ProfileId = expected[3].Id;

            var profileStore = Substitute.For<IProfileStore>();
            var linkStore = Substitute.For<ICategoryLinkStore>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new ProfileSearchManager(profileStore, linkStore, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                cache.GetProfileResults().Returns(expected);
                cache.GetCategoryLinks(filters[0]).Returns(firstCategoryLinks.Select(y => y.ProfileId).ToList());
                cache.GetCategoryLinks(filters[1]).Returns(secondCategoryLinks.Select(y => y.ProfileId).ToList());

                var actual = (await sut.GetProfileResults(filters, tokenSource.Token).ConfigureAwait(false)).ToList();

                actual.Should().HaveCount(1);
                actual.Should().Contain(expected[3]);
                actual.Should().NotContain(expected[5]);
            }
        }

        [Fact]
        public async Task GetProfileResultsReturnsAllCachedResultsWhenFiltersIsEmptyTest()
        {
            var expected = Model.Create<List<ProfileResult>>();
            var filters = new List<ProfileFilter>();

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
            var filters = new List<ProfileFilter>();

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
        public async Task GetProfileResultsReturnsEmptyWhenFirstFilterFindsNoMatchesTest()
        {
            var expected = Model.Create<List<ProfileResult>>();
            var filters = new List<ProfileFilter>
            {
                Model.Create<ProfileFilter>(),
                Model.Create<ProfileFilter>()
            };
            var firstCategoryLinks = Model.Create<List<CategoryLink>>().SetEach(x =>
            {
                x.CategoryGroup = filters[0].CategoryGroup;
                x.CategoryName = filters[0].CategoryName;
            });
            var secondCategoryLinks = Model.Create<List<CategoryLink>>().SetEach(x =>
            {
                x.CategoryGroup = filters[1].CategoryGroup;
                x.CategoryName = filters[1].CategoryName;
            });

            secondCategoryLinks[2].ProfileId = expected[3].Id;
            secondCategoryLinks[8].ProfileId = expected[5].Id;

            var profileStore = Substitute.For<IProfileStore>();
            var linkStore = Substitute.For<ICategoryLinkStore>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new ProfileSearchManager(profileStore, linkStore, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                cache.GetProfileResults().Returns(expected);
                cache.GetCategoryLinks(filters[0]).Returns(firstCategoryLinks.Select(y => y.ProfileId).ToList());
                cache.GetCategoryLinks(filters[1]).Returns(secondCategoryLinks.Select(y => y.ProfileId).ToList());

                var actual = (await sut.GetProfileResults(filters, tokenSource.Token).ConfigureAwait(false)).ToList();

                actual.Should().BeEmpty();
            }
        }

        [Fact]
        public async Task GetProfileResultsReturnsEmptyWhenLastCategoryLinksHasNoMatchesTest()
        {
            var expected = Model.Create<List<ProfileResult>>();
            var filters = new List<ProfileFilter>
            {
                Model.Create<ProfileFilter>(),
                Model.Create<ProfileFilter>(),
                Model.Create<ProfileFilter>(),
                Model.Create<ProfileFilter>()
            };
            var firstCategoryLinks = Model.Create<List<CategoryLink>>().SetEach(x =>
            {
                x.CategoryGroup = filters[0].CategoryGroup;
                x.CategoryName = filters[0].CategoryName;
            });
            var secondCategoryLinks = Model.Create<List<CategoryLink>>().SetEach(x =>
            {
                x.CategoryGroup = filters[1].CategoryGroup;
                x.CategoryName = filters[1].CategoryName;
            });
            var thirdCategoryLinks = Model.Create<List<CategoryLink>>().SetEach(x =>
            {
                x.CategoryGroup = filters[2].CategoryGroup;
                x.CategoryName = filters[2].CategoryName;
            });
            var fourthCategoryLinks = Model.Create<List<CategoryLink>>().SetEach(x =>
            {
                x.CategoryGroup = filters[3].CategoryGroup;
                x.CategoryName = filters[3].CategoryName;
            });

            firstCategoryLinks[7].ProfileId = expected[3].Id;
            firstCategoryLinks[3].ProfileId = expected[5].Id;
            secondCategoryLinks[2].ProfileId = expected[3].Id;
            secondCategoryLinks[8].ProfileId = expected[5].Id;
            thirdCategoryLinks[1].ProfileId = expected[3].Id;
            thirdCategoryLinks[9].ProfileId = expected[5].Id;

            var profileStore = Substitute.For<IProfileStore>();
            var linkStore = Substitute.For<ICategoryLinkStore>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new ProfileSearchManager(profileStore, linkStore, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                cache.GetProfileResults().Returns(expected);
                cache.GetCategoryLinks(filters[0]).Returns(firstCategoryLinks.Select(y => y.ProfileId).ToList());
                cache.GetCategoryLinks(filters[1]).Returns(secondCategoryLinks.Select(y => y.ProfileId).ToList());
                cache.GetCategoryLinks(filters[2]).Returns(thirdCategoryLinks.Select(y => y.ProfileId).ToList());
                cache.GetCategoryLinks(filters[3]).Returns(fourthCategoryLinks.Select(y => y.ProfileId).ToList());

                var actual = (await sut.GetProfileResults(filters, tokenSource.Token).ConfigureAwait(false)).ToList();

                actual.Should().BeEmpty();
            }
        }

        [Fact]
        public async Task GetProfileResultsReturnsEmptyWhenNoProfilesMatchFiltersTest()
        {
            var expected = Model.Create<List<ProfileResult>>();
            var filters = new List<ProfileFilter>
            {
                Model.Create<ProfileFilter>()
            };
            var categoryLinks = Model.Create<List<CategoryLink>>();

            var profileStore = Substitute.For<IProfileStore>();
            var linkStore = Substitute.For<ICategoryLinkStore>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new ProfileSearchManager(profileStore, linkStore, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                cache.GetProfileResults().Returns(expected);
                cache.GetCategoryLinks(Arg.Any<ProfileFilter>()).Returns((ICollection<Guid>) null);
                linkStore.GetCategoryLinks(filters[0].CategoryGroup, filters[0].CategoryName, tokenSource.Token)
                    .Returns(categoryLinks);

                var actual = await sut.GetProfileResults(filters, tokenSource.Token).ConfigureAwait(false);

                actual.Should().BeEmpty();
            }
        }

        [Fact]
        public async Task GetProfileResultsReturnsEmptyWhenOnlyFilterHasNoLinksTest()
        {
            var expected = Model.Create<List<ProfileResult>>();
            var filters = new List<ProfileFilter>
            {
                Model.Create<ProfileFilter>()
            };
            var categoryLinks = new List<CategoryLink>();

            var profileStore = Substitute.For<IProfileStore>();
            var linkStore = Substitute.For<ICategoryLinkStore>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new ProfileSearchManager(profileStore, linkStore, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                cache.GetProfileResults().Returns(expected);
                cache.GetCategoryLinks(filters[0]).Returns(categoryLinks.Select(y => y.ProfileId).ToList());

                var actual = (await sut.GetProfileResults(filters, tokenSource.Token).ConfigureAwait(false)).ToList();

                actual.Should().BeEmpty();
            }
        }

        [Fact]
        public async Task GetProfileResultsReturnsEmptyWhenSecondFilterFindsNoMatchesTest()
        {
            var expected = Model.Create<List<ProfileResult>>();
            var filters = new List<ProfileFilter>
            {
                Model.Create<ProfileFilter>(),
                Model.Create<ProfileFilter>()
            };
            var firstCategoryLinks = Model.Create<List<CategoryLink>>().SetEach(x =>
            {
                x.CategoryGroup = filters[0].CategoryGroup;
                x.CategoryName = filters[0].CategoryName;
            });
            var secondCategoryLinks = Model.Create<List<CategoryLink>>().SetEach(x =>
            {
                x.CategoryGroup = filters[1].CategoryGroup;
                x.CategoryName = filters[1].CategoryName;
            });

            firstCategoryLinks[7].ProfileId = expected[3].Id;
            firstCategoryLinks[3].ProfileId = expected[5].Id;

            var profileStore = Substitute.For<IProfileStore>();
            var linkStore = Substitute.For<ICategoryLinkStore>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new ProfileSearchManager(profileStore, linkStore, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                cache.GetProfileResults().Returns(expected);
                cache.GetCategoryLinks(filters[0]).Returns(firstCategoryLinks.Select(y => y.ProfileId).ToList());
                cache.GetCategoryLinks(filters[1]).Returns(secondCategoryLinks.Select(y => y.ProfileId).ToList());

                var actual = (await sut.GetProfileResults(filters, tokenSource.Token).ConfigureAwait(false)).ToList();

                actual.Should().BeEmpty();
            }
        }

        [Fact]
        public async Task GetProfileResultsReturnsEmptyWhenSecondLastCategoryLinksHasNoMatchesTest()
        {
            var expected = Model.Create<List<ProfileResult>>();
            var filters = new List<ProfileFilter>
            {
                Model.Create<ProfileFilter>(),
                Model.Create<ProfileFilter>(),
                Model.Create<ProfileFilter>(),
                Model.Create<ProfileFilter>()
            };
            var firstCategoryLinks = Model.Create<List<CategoryLink>>().SetEach(x =>
            {
                x.CategoryGroup = filters[0].CategoryGroup;
                x.CategoryName = filters[0].CategoryName;
            });
            var secondCategoryLinks = Model.Create<List<CategoryLink>>().SetEach(x =>
            {
                x.CategoryGroup = filters[1].CategoryGroup;
                x.CategoryName = filters[1].CategoryName;
            });
            var thirdCategoryLinks = Model.Create<List<CategoryLink>>().SetEach(x =>
            {
                x.CategoryGroup = filters[2].CategoryGroup;
                x.CategoryName = filters[2].CategoryName;
            });
            var fourthCategoryLinks = Model.Create<List<CategoryLink>>().SetEach(x =>
            {
                x.CategoryGroup = filters[3].CategoryGroup;
                x.CategoryName = filters[3].CategoryName;
            });

            firstCategoryLinks[7].ProfileId = expected[3].Id;
            firstCategoryLinks[3].ProfileId = expected[5].Id;
            secondCategoryLinks[2].ProfileId = expected[3].Id;
            secondCategoryLinks[8].ProfileId = expected[5].Id;
            fourthCategoryLinks[8].ProfileId = expected[3].Id;
            fourthCategoryLinks[2].ProfileId = expected[5].Id;

            var profileStore = Substitute.For<IProfileStore>();
            var linkStore = Substitute.For<ICategoryLinkStore>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new ProfileSearchManager(profileStore, linkStore, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                cache.GetProfileResults().Returns(expected);
                cache.GetCategoryLinks(filters[0]).Returns(firstCategoryLinks.Select(y => y.ProfileId).ToList());
                cache.GetCategoryLinks(filters[1]).Returns(secondCategoryLinks.Select(y => y.ProfileId).ToList());
                cache.GetCategoryLinks(filters[2]).Returns(thirdCategoryLinks.Select(y => y.ProfileId).ToList());
                cache.GetCategoryLinks(filters[3]).Returns(fourthCategoryLinks.Select(y => y.ProfileId).ToList());

                var actual = (await sut.GetProfileResults(filters, tokenSource.Token).ConfigureAwait(false)).ToList();

                actual.Should().BeEmpty();
            }
        }

        [Fact]
        public async Task GetProfileResultsReturnsIgnoresFiltersWithEmptyListsTest()
        {
            var expected = Model.Create<List<ProfileResult>>();
            var filters = new List<ProfileFilter>
            {
                Model.Create<ProfileFilter>(),
                Model.Create<ProfileFilter>()
            };
            var firstCategoryLinks = Model.Create<List<CategoryLink>>().SetEach(x =>
            {
                x.CategoryGroup = filters[0].CategoryGroup;
                x.CategoryName = filters[0].CategoryName;
            });
            var secondCategoryLinks = new List<CategoryLink>();

            firstCategoryLinks[7].ProfileId = expected[3].Id;
            firstCategoryLinks[3].ProfileId = expected[5].Id;

            var profileStore = Substitute.For<IProfileStore>();
            var linkStore = Substitute.For<ICategoryLinkStore>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new ProfileSearchManager(profileStore, linkStore, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                cache.GetProfileResults().Returns(expected);
                cache.GetCategoryLinks(filters[0]).Returns(firstCategoryLinks.Select(y => y.ProfileId).ToList());
                cache.GetCategoryLinks(filters[1]).Returns(secondCategoryLinks.Select(y => y.ProfileId).ToList());

                var actual = (await sut.GetProfileResults(filters, tokenSource.Token).ConfigureAwait(false)).ToList();

                actual.Should().HaveCount(2);
                actual.Should().Contain(expected[3]);
                actual.Should().Contain(expected[5]);
            }
        }

        [Fact]
        public async Task GetProfileResultsReturnsProfilesMatchingAllFiltersTest()
        {
            var expected = Model.Create<List<ProfileResult>>();
            var filters = new List<ProfileFilter>
            {
                Model.Create<ProfileFilter>(),
                Model.Create<ProfileFilter>()
            };
            var firstCategoryLinks = Model.Create<List<CategoryLink>>().SetEach(x =>
            {
                x.CategoryGroup = filters[0].CategoryGroup;
                x.CategoryName = filters[0].CategoryName;
            });
            var secondCategoryLinks = Model.Create<List<CategoryLink>>().SetEach(x =>
            {
                x.CategoryGroup = filters[1].CategoryGroup;
                x.CategoryName = filters[1].CategoryName;
            });

            firstCategoryLinks[7].ProfileId = expected[3].Id;
            firstCategoryLinks[3].ProfileId = expected[5].Id;
            secondCategoryLinks[2].ProfileId = expected[3].Id;
            secondCategoryLinks[8].ProfileId = expected[5].Id;

            var profileStore = Substitute.For<IProfileStore>();
            var linkStore = Substitute.For<ICategoryLinkStore>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new ProfileSearchManager(profileStore, linkStore, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                cache.GetProfileResults().Returns(expected);
                cache.GetCategoryLinks(filters[0]).Returns(firstCategoryLinks.Select(y => y.ProfileId).ToList());
                cache.GetCategoryLinks(filters[1]).Returns(secondCategoryLinks.Select(y => y.ProfileId).ToList());

                var actual = (await sut.GetProfileResults(filters, tokenSource.Token).ConfigureAwait(false)).ToList();

                actual.Should().HaveCount(2);
                actual.Should().Contain(expected[3]);
                actual.Should().Contain(expected[5]);
            }
        }

        [Fact]
        public async Task GetProfileResultsReturnsProfilesMatchingMoreThanTwoFiltersTest()
        {
            var expected = Model.Create<List<ProfileResult>>();
            var filters = new List<ProfileFilter>
            {
                Model.Create<ProfileFilter>(),
                Model.Create<ProfileFilter>(),
                Model.Create<ProfileFilter>(),
                Model.Create<ProfileFilter>()
            };
            var firstCategoryLinks = Model.Create<List<CategoryLink>>().SetEach(x =>
            {
                x.CategoryGroup = filters[0].CategoryGroup;
                x.CategoryName = filters[0].CategoryName;
            });
            var secondCategoryLinks = Model.Create<List<CategoryLink>>().SetEach(x =>
            {
                x.CategoryGroup = filters[1].CategoryGroup;
                x.CategoryName = filters[1].CategoryName;
            });
            var thirdCategoryLinks = Model.Create<List<CategoryLink>>().SetEach(x =>
            {
                x.CategoryGroup = filters[2].CategoryGroup;
                x.CategoryName = filters[2].CategoryName;
            });
            var fourthCategoryLinks = Model.Create<List<CategoryLink>>().SetEach(x =>
            {
                x.CategoryGroup = filters[3].CategoryGroup;
                x.CategoryName = filters[3].CategoryName;
            });

            firstCategoryLinks[7].ProfileId = expected[3].Id;
            firstCategoryLinks[3].ProfileId = expected[5].Id;
            secondCategoryLinks[2].ProfileId = expected[3].Id;
            secondCategoryLinks[8].ProfileId = expected[5].Id;
            thirdCategoryLinks[1].ProfileId = expected[3].Id;
            thirdCategoryLinks[9].ProfileId = expected[5].Id;
            fourthCategoryLinks[8].ProfileId = expected[3].Id;
            fourthCategoryLinks[2].ProfileId = expected[5].Id;

            var profileStore = Substitute.For<IProfileStore>();
            var linkStore = Substitute.For<ICategoryLinkStore>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new ProfileSearchManager(profileStore, linkStore, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                cache.GetProfileResults().Returns(expected);
                cache.GetCategoryLinks(filters[0]).Returns(firstCategoryLinks.Select(y => y.ProfileId).ToList());
                cache.GetCategoryLinks(filters[1]).Returns(secondCategoryLinks.Select(y => y.ProfileId).ToList());
                cache.GetCategoryLinks(filters[2]).Returns(thirdCategoryLinks.Select(y => y.ProfileId).ToList());
                cache.GetCategoryLinks(filters[3]).Returns(fourthCategoryLinks.Select(y => y.ProfileId).ToList());

                var actual = (await sut.GetProfileResults(filters, tokenSource.Token).ConfigureAwait(false)).ToList();

                actual.Should().HaveCount(2);
                actual.Should().Contain(expected[3]);
                actual.Should().Contain(expected[5]);
            }
        }

        [Fact]
        public async Task GetProfileResultsReturnsProfilesMatchingSingleFilterTest()
        {
            var expected = Model.Create<List<ProfileResult>>();
            var filters = new List<ProfileFilter>
            {
                Model.Create<ProfileFilter>()
            };
            var categoryLinks = Model.Create<List<CategoryLink>>().SetEach(x =>
            {
                x.CategoryGroup = filters[0].CategoryGroup;
                x.CategoryName = filters[0].CategoryName;
            });

            categoryLinks[7].ProfileId = expected[3].Id;
            categoryLinks[3].ProfileId = expected[5].Id;

            var profileStore = Substitute.For<IProfileStore>();
            var linkStore = Substitute.For<ICategoryLinkStore>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new ProfileSearchManager(profileStore, linkStore, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                cache.GetProfileResults().Returns(expected);
                cache.GetCategoryLinks(filters[0]).Returns(categoryLinks.Select(y => y.ProfileId).ToList());

                var actual = (await sut.GetProfileResults(filters, tokenSource.Token).ConfigureAwait(false)).ToList();

                actual.Should().HaveCount(2);
                actual.Should().Contain(expected[3]);
                actual.Should().Contain(expected[5]);
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

            var filters = new List<ProfileFilter>();

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
        public async Task GetProfileResultsReusesCachedCategoryLinksTest()
        {
            var expected = Model.Create<List<ProfileResult>>();
            var filters = new List<ProfileFilter>
            {
                Model.Create<ProfileFilter>()
            };
            var categoryLinks = Model.Create<List<CategoryLink>>();

            var profileStore = Substitute.For<IProfileStore>();
            var linkStore = Substitute.For<ICategoryLinkStore>();
            var cache = Substitute.For<ICacheManager>();

            var sut = new ProfileSearchManager(profileStore, linkStore, cache);

            using (var tokenSource = new CancellationTokenSource())
            {
                cache.GetProfileResults().Returns(expected);
                cache.GetCategoryLinks(Arg.Any<ProfileFilter>())
                    .Returns(null, categoryLinks.Select(y => y.ProfileId).ToList());
                linkStore.GetCategoryLinks(filters[0].CategoryGroup, filters[0].CategoryName, tokenSource.Token)
                    .Returns(categoryLinks);

                await sut.GetProfileResults(filters, tokenSource.Token).ConfigureAwait(false);
                await sut.GetProfileResults(filters, tokenSource.Token).ConfigureAwait(false);

                await linkStore.Received(1).GetCategoryLinks(filters[0].CategoryGroup, filters[0].CategoryName,
                    tokenSource.Token).ConfigureAwait(false);
                cache.Received(1).StoreCategoryLinks(filters[0],
                    Verify.That<ICollection<Guid>>(
                        x => x.ShouldBeEquivalentTo(categoryLinks.Select(y => y.ProfileId))));
                cache.Received(2).GetCategoryLinks(filters[0]);
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