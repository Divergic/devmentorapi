namespace TechMentorApi.Business.UnitTests.Queries
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
    using TechMentorApi.Business.Queries;
    using TechMentorApi.Model;
    using Xunit;

    public class ProfileSearchQueryTests
    {
        [Fact]
        public async Task GetProfileResultsCachesCategoryLinksTest()
        {
            var expected = Model.Create<List<ProfileResult>>();
            var filters = new List<ProfileFilter>
                {Model.Create<ProfileFilter>()};
            var categoryLinks = Model.Create<List<CategoryLink>>();
            var categories = from x in filters
                             select new Category
                             {
                                 Group = x.CategoryGroup,
                                 Name = x.CategoryName,
                                 Visible = true
                             };

            var profileStore = Substitute.For<IProfileStore>();
            var linkStore = Substitute.For<ICategoryLinkStore>();
            var cache = Substitute.For<ICacheManager>();
            var query = Substitute.For<ICategoryQuery>();

            var sut = new ProfileSearchQuery(profileStore, linkStore, cache, query);

            using (var tokenSource = new CancellationTokenSource())
            {
                query.GetCategories(ReadType.VisibleOnly, tokenSource.Token).Returns(categories);
                cache.GetProfileResults().Returns(expected);
                cache.GetCategoryLinks(Arg.Any<ProfileFilter>()).Returns((ICollection<Guid>)null);
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
            var filterCategories = from x in filters
                                   select new Category
                                   {
                                       Group = x.CategoryGroup,
                                       Name = x.CategoryName,
                                       Visible = true
                                   };
            var genderCategories = from x in expected
                                   select new Category
                                   {
                                       Group = CategoryGroup.Gender,
                                       Name = x.Gender,
                                       Visible = true
                                   };
            var categories = filterCategories.Union(genderCategories);

            firstCategoryLinks[7].ProfileId = expected[3].Id;
            firstCategoryLinks[3].ProfileId = expected[5].Id;
            secondCategoryLinks[2].ProfileId = expected[3].Id;

            var profileStore = Substitute.For<IProfileStore>();
            var linkStore = Substitute.For<ICategoryLinkStore>();
            var cache = Substitute.For<ICacheManager>();
            var query = Substitute.For<ICategoryQuery>();

            var sut = new ProfileSearchQuery(profileStore, linkStore, cache, query);

            using (var tokenSource = new CancellationTokenSource())
            {
                query.GetCategories(ReadType.VisibleOnly, tokenSource.Token).Returns(categories);
                cache.GetProfileResults().Returns(expected);
                cache.GetCategoryLinks(filters[0]).Returns(firstCategoryLinks.Select(y => y.ProfileId).ToList());
                cache.GetCategoryLinks(filters[1]).Returns(secondCategoryLinks.Select(y => y.ProfileId).ToList());

                var actual = (await sut.GetProfileResults(filters, tokenSource.Token).ConfigureAwait(false)).ToList();

                actual.Should().HaveCount(1);
                actual.Single(x => x.Id == expected[3].Id).ShouldBeEquivalentTo(expected[3]);
                actual.Should().NotContain(x => x.Id == expected[5].Id);
            }
        }

        [Fact]
        public async Task GetProfileResultsIgnoresCheckingUnapprovedGendersForProfilesThatLackGenderValueTest()
        {
            var expected = Model.Create<List<ProfileResult>>().SetEach(x => x.Gender = null);
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

            var categories = from x in expected
                             select new Category
                             {
                                 Group = CategoryGroup.Gender,
                                 LinkCount = 1,
                                 Name = Guid.NewGuid().ToString(),
                                 Reviewed = true,
                                 Visible = true
                             };

            var profileStore = Substitute.For<IProfileStore>();
            var linkStore = Substitute.For<ICategoryLinkStore>();
            var cache = Substitute.For<ICacheManager>();
            var query = Substitute.For<ICategoryQuery>();

            var sut = new ProfileSearchQuery(profileStore, linkStore, cache, query);

            using (var tokenSource = new CancellationTokenSource())
            {
                cache.GetProfileResults().Returns((ICollection<ProfileResult>)null);
                profileStore.GetProfileResults(tokenSource.Token).Returns(expected);
                cache.GetCategoryLinks(filters[0]).Returns(firstCategoryLinks.Select(y => y.ProfileId).ToList());
                cache.GetCategoryLinks(filters[1]).Returns(secondCategoryLinks.Select(y => y.ProfileId).ToList());
                query.GetCategories(ReadType.VisibleOnly, tokenSource.Token).Returns(categories);

                var actual = await sut.GetProfileResults(filters, tokenSource.Token).ConfigureAwait(false);

                actual.All(x => x.Gender == null).Should().BeTrue();
            }
        }

        [Fact]
        public async Task GetProfileResultsIgnoresFiltersWithEmptyListsTest()
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
            var filterCategories = from x in filters
                                   select new Category
                                   {
                                       Group = x.CategoryGroup,
                                       Name = x.CategoryName,
                                       Visible = true
                                   };
            var genderCategories = from x in expected
                                   select new Category
                                   {
                                       Group = CategoryGroup.Gender,
                                       Name = x.Gender,
                                       Visible = true
                                   };
            var categories = filterCategories.Union(genderCategories);

            firstCategoryLinks[7].ProfileId = expected[3].Id;
            firstCategoryLinks[3].ProfileId = expected[5].Id;

            var profileStore = Substitute.For<IProfileStore>();
            var linkStore = Substitute.For<ICategoryLinkStore>();
            var cache = Substitute.For<ICacheManager>();
            var query = Substitute.For<ICategoryQuery>();

            var sut = new ProfileSearchQuery(profileStore, linkStore, cache, query);

            using (var tokenSource = new CancellationTokenSource())
            {
                cache.GetProfileResults().Returns(expected);
                cache.GetCategoryLinks(filters[0]).Returns(firstCategoryLinks.Select(y => y.ProfileId).ToList());
                cache.GetCategoryLinks(filters[1]).Returns(secondCategoryLinks.Select(y => y.ProfileId).ToList());
                query.GetCategories(ReadType.VisibleOnly, tokenSource.Token).Returns(categories);

                var actual = (await sut.GetProfileResults(filters, tokenSource.Token).ConfigureAwait(false)).ToList();

                actual.Should().HaveCount(2);
                actual.Single(x => x.Id == expected[3].Id).ShouldBeEquivalentTo(expected[3]);
                actual.Single(x => x.Id == expected[5].Id).ShouldBeEquivalentTo(expected[5]);
            }
        }

        [Fact]
        public async Task GetProfileResultsRemovesGenderWhenNoMatchOnApprovedGenderTest()
        {
            var expected = Model.Create<List<ProfileResult>>().SetEach(x => x.Gender = Guid.NewGuid().ToString());
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

            var categories = from x in expected
                             select new Category
                             {
                                 Group = CategoryGroup.Gender,
                                 LinkCount = 1,
                                 Name = Guid.NewGuid().ToString(),
                                 Reviewed = true,
                                 Visible = true
                             };

            var profileStore = Substitute.For<IProfileStore>();
            var linkStore = Substitute.For<ICategoryLinkStore>();
            var cache = Substitute.For<ICacheManager>();
            var query = Substitute.For<ICategoryQuery>();

            var sut = new ProfileSearchQuery(profileStore, linkStore, cache, query);

            using (var tokenSource = new CancellationTokenSource())
            {
                cache.GetProfileResults().Returns((ICollection<ProfileResult>)null);
                profileStore.GetProfileResults(tokenSource.Token).Returns(expected);
                cache.GetCategoryLinks(filters[0]).Returns(firstCategoryLinks.Select(y => y.ProfileId).ToList());
                cache.GetCategoryLinks(filters[1]).Returns(secondCategoryLinks.Select(y => y.ProfileId).ToList());
                query.GetCategories(ReadType.VisibleOnly, tokenSource.Token).Returns(categories);

                var actual = await sut.GetProfileResults(filters, tokenSource.Token).ConfigureAwait(false);

                actual.All(x => x.Gender == null).Should().BeTrue();
            }
        }

        [Fact]
        public async Task GetProfileResultsReturnsAllCachedResultsWhenFiltersIsEmptyTest()
        {
            var expected = Model.Create<List<ProfileResult>>();
            var filters = new List<ProfileFilter>();
            var categories = from x in expected
                             select new Category
                             {
                                 Group = CategoryGroup.Gender,
                                 Name = x.Gender,
                                 Visible = true
                             };

            var profileStore = Substitute.For<IProfileStore>();
            var linkStore = Substitute.For<ICategoryLinkStore>();
            var cache = Substitute.For<ICacheManager>();
            var query = Substitute.For<ICategoryQuery>();

            cache.GetProfileResults().Returns(expected);

            var sut = new ProfileSearchQuery(profileStore, linkStore, cache, query);

            using (var tokenSource = new CancellationTokenSource())
            {
                query.GetCategories(ReadType.VisibleOnly, tokenSource.Token).Returns(categories);

                var actual = await sut.GetProfileResults(filters, tokenSource.Token).ConfigureAwait(false);

                actual.ShouldBeEquivalentTo(expected);
            }
        }

        [Fact]
        public async Task GetProfileResultsReturnsAllCachedResultsWhenFiltersIsNullTest()
        {
            var expected = Model.Create<List<ProfileResult>>();
            var categories = from x in expected
                             select new Category
                             {
                                 Group = CategoryGroup.Gender,
                                 Name = x.Gender,
                                 Visible = true
                             };

            var profileStore = Substitute.For<IProfileStore>();
            var linkStore = Substitute.For<ICategoryLinkStore>();
            var cache = Substitute.For<ICacheManager>();
            var query = Substitute.For<ICategoryQuery>();

            cache.GetProfileResults().Returns(expected);

            var sut = new ProfileSearchQuery(profileStore, linkStore, cache, query);

            using (var tokenSource = new CancellationTokenSource())
            {
                query.GetCategories(ReadType.VisibleOnly, tokenSource.Token).Returns(categories);

                var actual = await sut.GetProfileResults(null, tokenSource.Token).ConfigureAwait(false);

                actual.ShouldBeEquivalentTo(expected);
            }
        }

        [Fact]
        public async Task GetProfileResultsReturnsAllStoreResultsWhenFiltersIsEmptyTest()
        {
            var expected = Model.Create<List<ProfileResult>>();
            var filters = new List<ProfileFilter>();
            var categories = from x in expected
                             select new Category
                             {
                                 Group = CategoryGroup.Gender,
                                 Name = x.Gender,
                                 Visible = true
                             };

            var profileStore = Substitute.For<IProfileStore>();
            var linkStore = Substitute.For<ICategoryLinkStore>();
            var cache = Substitute.For<ICacheManager>();
            var query = Substitute.For<ICategoryQuery>();

            var sut = new ProfileSearchQuery(profileStore, linkStore, cache, query);

            using (var tokenSource = new CancellationTokenSource())
            {
                cache.GetProfileResults().Returns((ICollection<ProfileResult>)null);
                profileStore.GetProfileResults(tokenSource.Token).Returns(expected);
                query.GetCategories(ReadType.VisibleOnly, tokenSource.Token).Returns(categories);

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

            var categories = from x in expected
                             select new Category
                             {
                                 Group = CategoryGroup.Gender,
                                 Name = x.Gender,
                                 Visible = true
                             };

            var profileStore = Substitute.For<IProfileStore>();
            var linkStore = Substitute.For<ICategoryLinkStore>();
            var cache = Substitute.For<ICacheManager>();
            var query = Substitute.For<ICategoryQuery>();

            var sut = new ProfileSearchQuery(profileStore, linkStore, cache, query);

            using (var tokenSource = new CancellationTokenSource())
            {
                cache.GetProfileResults().Returns((ICollection<ProfileResult>)null);
                profileStore.GetProfileResults(tokenSource.Token).Returns(expected);
                query.GetCategories(ReadType.VisibleOnly, tokenSource.Token).Returns(categories);

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

            var categories = from x in expected
                             select new Category
                             {
                                 Group = CategoryGroup.Gender,
                                 Name = x.Gender,
                                 Visible = true
                             };

            var profileStore = Substitute.For<IProfileStore>();
            var linkStore = Substitute.For<ICategoryLinkStore>();
            var cache = Substitute.For<ICacheManager>();
            var query = Substitute.For<ICategoryQuery>();

            var sut = new ProfileSearchQuery(profileStore, linkStore, cache, query);

            using (var tokenSource = new CancellationTokenSource())
            {
                query.GetCategories(ReadType.VisibleOnly, tokenSource.Token).Returns(categories);
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

            var categories = from x in expected
                             select new Category
                             {
                                 Group = CategoryGroup.Gender,
                                 Name = x.Gender,
                                 Visible = true
                             };

            var profileStore = Substitute.For<IProfileStore>();
            var linkStore = Substitute.For<ICategoryLinkStore>();
            var cache = Substitute.For<ICacheManager>();
            var query = Substitute.For<ICategoryQuery>();

            var sut = new ProfileSearchQuery(profileStore, linkStore, cache, query);

            using (var tokenSource = new CancellationTokenSource())
            {
                query.GetCategories(ReadType.VisibleOnly, tokenSource.Token).Returns(categories);
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
                {Model.Create<ProfileFilter>()};
            var categoryLinks = Model.Create<List<CategoryLink>>();
            var categories = from x in expected
                             select new Category
                             {
                                 Group = CategoryGroup.Gender,
                                 Name = x.Gender,
                                 Visible = true
                             };

            var profileStore = Substitute.For<IProfileStore>();
            var linkStore = Substitute.For<ICategoryLinkStore>();
            var cache = Substitute.For<ICacheManager>();
            var query = Substitute.For<ICategoryQuery>();

            var sut = new ProfileSearchQuery(profileStore, linkStore, cache, query);

            using (var tokenSource = new CancellationTokenSource())
            {
                query.GetCategories(ReadType.VisibleOnly, tokenSource.Token).Returns(categories);
                cache.GetProfileResults().Returns(expected);
                cache.GetCategoryLinks(Arg.Any<ProfileFilter>()).Returns((ICollection<Guid>)null);
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
                {Model.Create<ProfileFilter>()};
            var categoryLinks = new List<CategoryLink>();
            var categories = from x in expected
                             select new Category
                             {
                                 Group = CategoryGroup.Gender,
                                 Name = x.Gender,
                                 Visible = true
                             };

            var profileStore = Substitute.For<IProfileStore>();
            var linkStore = Substitute.For<ICategoryLinkStore>();
            var cache = Substitute.For<ICacheManager>();
            var query = Substitute.For<ICategoryQuery>();

            var sut = new ProfileSearchQuery(profileStore, linkStore, cache, query);

            using (var tokenSource = new CancellationTokenSource())
            {
                query.GetCategories(ReadType.VisibleOnly, tokenSource.Token).Returns(categories);
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

            var categories = from x in expected
                             select new Category
                             {
                                 Group = CategoryGroup.Gender,
                                 Name = x.Gender,
                                 Visible = true
                             };

            var profileStore = Substitute.For<IProfileStore>();
            var linkStore = Substitute.For<ICategoryLinkStore>();
            var cache = Substitute.For<ICacheManager>();
            var query = Substitute.For<ICategoryQuery>();

            var sut = new ProfileSearchQuery(profileStore, linkStore, cache, query);

            using (var tokenSource = new CancellationTokenSource())
            {
                query.GetCategories(ReadType.VisibleOnly, tokenSource.Token).Returns(categories);
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

            var categories = from x in expected
                             select new Category
                             {
                                 Group = CategoryGroup.Gender,
                                 Name = x.Gender,
                                 Visible = true
                             };

            var profileStore = Substitute.For<IProfileStore>();
            var linkStore = Substitute.For<ICategoryLinkStore>();
            var cache = Substitute.For<ICacheManager>();
            var query = Substitute.For<ICategoryQuery>();

            var sut = new ProfileSearchQuery(profileStore, linkStore, cache, query);

            using (var tokenSource = new CancellationTokenSource())
            {
                query.GetCategories(ReadType.VisibleOnly, tokenSource.Token).Returns(categories);
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
        public async Task GetProfileResultsReturnsNewInstanceOfProfileResultsFromCacheToAvoidCacheCorruptionTest()
        {
            var expected = Model.Create<List<ProfileResult>>();
            var filters = new List<ProfileFilter>();
            var categories = from x in expected
                             select new Category
                             {
                                 Group = CategoryGroup.Gender,
                                 Name = x.Gender,
                                 Visible = true
                             };

            var profileStore = Substitute.For<IProfileStore>();
            var linkStore = Substitute.For<ICategoryLinkStore>();
            var cache = Substitute.For<ICacheManager>();
            var query = Substitute.For<ICategoryQuery>();

            cache.GetProfileResults().Returns(expected);

            var sut = new ProfileSearchQuery(profileStore, linkStore, cache, query);

            using (var tokenSource = new CancellationTokenSource())
            {
                query.GetCategories(ReadType.VisibleOnly, tokenSource.Token).Returns(categories);

                var actual = await sut.GetProfileResults(filters, tokenSource.Token).ConfigureAwait(false);

                actual.Should().NotBeSameAs(expected);
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
            var filterCategories = from x in filters
                                   select new Category
                                   {
                                       Group = x.CategoryGroup,
                                       Name = x.CategoryName,
                                       Visible = true
                                   };
            var genderCategories = from x in expected
                                   select new Category
                                   {
                                       Group = CategoryGroup.Gender,
                                       Name = x.Gender,
                                       Visible = true
                                   };
            var categories = filterCategories.Union(genderCategories);

            firstCategoryLinks[7].ProfileId = expected[3].Id;
            firstCategoryLinks[3].ProfileId = expected[5].Id;
            secondCategoryLinks[2].ProfileId = expected[3].Id;
            secondCategoryLinks[8].ProfileId = expected[5].Id;

            var profileStore = Substitute.For<IProfileStore>();
            var linkStore = Substitute.For<ICategoryLinkStore>();
            var cache = Substitute.For<ICacheManager>();
            var query = Substitute.For<ICategoryQuery>();

            var sut = new ProfileSearchQuery(profileStore, linkStore, cache, query);

            using (var tokenSource = new CancellationTokenSource())
            {
                cache.GetProfileResults().Returns(expected);
                cache.GetCategoryLinks(filters[0]).Returns(firstCategoryLinks.Select(y => y.ProfileId).ToList());
                cache.GetCategoryLinks(filters[1]).Returns(secondCategoryLinks.Select(y => y.ProfileId).ToList());
                query.GetCategories(ReadType.VisibleOnly, tokenSource.Token).Returns(categories);

                var actual = (await sut.GetProfileResults(filters, tokenSource.Token).ConfigureAwait(false)).ToList();

                actual.Should().HaveCount(2);
                actual.Single(x => x.Id == expected[3].Id).ShouldBeEquivalentTo(expected[3]);
                actual.Single(x => x.Id == expected[5].Id).ShouldBeEquivalentTo(expected[5]);
            }
        }

        [Fact]
        public async Task GetProfileIgnoresFiltersForUnapprovedCategoriesTest()
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
            var filterCategories = new[]
                {
                    new Category
                    {
                        Group = filters[0].CategoryGroup,
                        Name = filters[0].CategoryName
                    }
                };
            var genderCategories = from x in expected
                                   select new Category
                                   {
                                       Group = CategoryGroup.Gender,
                                       Name = x.Gender,
                                       Visible = true
                                   };
            var categories = filterCategories.Union(genderCategories);

            firstCategoryLinks[7].ProfileId = expected[3].Id;
            firstCategoryLinks[3].ProfileId = expected[5].Id;

            var profileStore = Substitute.For<IProfileStore>();
            var linkStore = Substitute.For<ICategoryLinkStore>();
            var cache = Substitute.For<ICacheManager>();
            var query = Substitute.For<ICategoryQuery>();

            var sut = new ProfileSearchQuery(profileStore, linkStore, cache, query);

            using (var tokenSource = new CancellationTokenSource())
            {
                cache.GetProfileResults().Returns(expected);
                cache.GetCategoryLinks(filters[0]).Returns(firstCategoryLinks.Select(y => y.ProfileId).ToList());
                query.GetCategories(ReadType.VisibleOnly, tokenSource.Token).Returns(categories);

                var actual = (await sut.GetProfileResults(filters, tokenSource.Token).ConfigureAwait(false)).ToList();

                actual.Should().HaveCount(2);
                actual.Single(x => x.Id == expected[3].Id).ShouldBeEquivalentTo(expected[3]);
                actual.Single(x => x.Id == expected[5].Id).ShouldBeEquivalentTo(expected[5]);
                
                cache.DidNotReceive().GetCategoryLinks(filters[1]);
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
            var filterCategories = from x in filters
                                   select new Category
                                   {
                                       Group = x.CategoryGroup,
                                       Name = x.CategoryName,
                                       Visible = true
                                   };
            var genderCategories = from x in expected
                                   select new Category
                                   {
                                       Group = CategoryGroup.Gender,
                                       Name = x.Gender,
                                       Visible = true
                                   };
            var categories = filterCategories.Union(genderCategories);

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
            var query = Substitute.For<ICategoryQuery>();

            var sut = new ProfileSearchQuery(profileStore, linkStore, cache, query);

            using (var tokenSource = new CancellationTokenSource())
            {
                cache.GetProfileResults().Returns(expected);
                cache.GetCategoryLinks(filters[0]).Returns(firstCategoryLinks.Select(y => y.ProfileId).ToList());
                cache.GetCategoryLinks(filters[1]).Returns(secondCategoryLinks.Select(y => y.ProfileId).ToList());
                cache.GetCategoryLinks(filters[2]).Returns(thirdCategoryLinks.Select(y => y.ProfileId).ToList());
                cache.GetCategoryLinks(filters[3]).Returns(fourthCategoryLinks.Select(y => y.ProfileId).ToList());
                query.GetCategories(ReadType.VisibleOnly, tokenSource.Token).Returns(categories);

                var actual = (await sut.GetProfileResults(filters, tokenSource.Token).ConfigureAwait(false)).ToList();

                actual.Should().HaveCount(2);
                actual.Single(x => x.Id == expected[3].Id).ShouldBeEquivalentTo(expected[3]);
                actual.Single(x => x.Id == expected[5].Id).ShouldBeEquivalentTo(expected[5]);
            }
        }

        [Fact]
        public async Task GetProfileResultsReturnsProfilesMatchingSingleFilterTest()
        {
            var expected = Model.Create<List<ProfileResult>>();
            var filters = new List<ProfileFilter>
                {Model.Create<ProfileFilter>()};
            var categoryLinks = Model.Create<List<CategoryLink>>().SetEach(x =>
            {
                x.CategoryGroup = filters[0].CategoryGroup;
                x.CategoryName = filters[0].CategoryName;
            });
            var filterCategories = from x in filters
                                   select new Category
                                   {
                                       Group = x.CategoryGroup,
                                       Name = x.CategoryName,
                                       Visible = true
                                   };
            var genderCategories = from x in expected
                                   select new Category
                                   {
                                       Group = CategoryGroup.Gender,
                                       Name = x.Gender,
                                       Visible = true
                                   };
            var categories = filterCategories.Union(genderCategories);

            categoryLinks[7].ProfileId = expected[3].Id;
            categoryLinks[3].ProfileId = expected[5].Id;

            var profileStore = Substitute.For<IProfileStore>();
            var linkStore = Substitute.For<ICategoryLinkStore>();
            var cache = Substitute.For<ICacheManager>();
            var query = Substitute.For<ICategoryQuery>();

            var sut = new ProfileSearchQuery(profileStore, linkStore, cache, query);

            using (var tokenSource = new CancellationTokenSource())
            {
                cache.GetProfileResults().Returns(expected);
                cache.GetCategoryLinks(filters[0]).Returns(categoryLinks.Select(y => y.ProfileId).ToList());
                query.GetCategories(ReadType.VisibleOnly, tokenSource.Token).Returns(categories);

                var actual = (await sut.GetProfileResults(filters, tokenSource.Token).ConfigureAwait(false)).ToList();

                actual.Should().HaveCount(2);
                actual.Single(x => x.Id == expected[3].Id).ShouldBeEquivalentTo(expected[3]);
                actual.Single(x => x.Id == expected[5].Id).ShouldBeEquivalentTo(expected[5]);
            }
        }

        [Theory]
        [InlineData("Female", "Female")]
        [InlineData("Female", "FEMALE")]
        [InlineData("Female", "female")]
        public async Task GetProfileResultsReturnsResultsWithCaseInsensitiveMatchOnApprovedGenderTest(
            string profileGender, string categoryGender)
        {
            var expected = new List<ProfileResult>
                {Model.Create<ProfileResult>().Set(x => x.Gender = profileGender)};
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

            firstCategoryLinks[7].ProfileId = expected[0].Id;
            secondCategoryLinks[2].ProfileId = expected[0].Id;

            var filterCategories = from x in filters
                                   select new Category
                                   {
                                       Group = x.CategoryGroup,
                                       Name = x.CategoryName,
                                       Visible = true
                                   };
            var genderCategories = from x in expected
                                   select new Category
                                   {
                                       Group = CategoryGroup.Gender,
                                       LinkCount = 1,
                                       Name = categoryGender,
                                       Reviewed = true,
                                       Visible = true
                                   };
            var categories = filterCategories.Union(genderCategories);

            var profileStore = Substitute.For<IProfileStore>();
            var linkStore = Substitute.For<ICategoryLinkStore>();
            var cache = Substitute.For<ICacheManager>();
            var query = Substitute.For<ICategoryQuery>();

            var sut = new ProfileSearchQuery(profileStore, linkStore, cache, query);

            using (var tokenSource = new CancellationTokenSource())
            {
                cache.GetProfileResults().Returns((ICollection<ProfileResult>)null);
                profileStore.GetProfileResults(tokenSource.Token).Returns(expected);
                cache.GetCategoryLinks(filters[0]).Returns(firstCategoryLinks.Select(y => y.ProfileId).ToList());
                cache.GetCategoryLinks(filters[1]).Returns(secondCategoryLinks.Select(y => y.ProfileId).ToList());
                query.GetCategories(ReadType.VisibleOnly, tokenSource.Token).Returns(categories);

                var actual = await sut.GetProfileResults(filters, tokenSource.Token).ConfigureAwait(false);

                actual.Single().Gender.Should().Be(profileGender);
            }
        }

        [Fact]
        public async Task GetProfileResultsReusesCachedCategoryLinksTest()
        {
            var expected = Model.Create<List<ProfileResult>>();
            var filters = new List<ProfileFilter>
                {Model.Create<ProfileFilter>()};
            var categoryLinks = Model.Create<List<CategoryLink>>();
            var filterCategories = from x in filters
                                   select new Category
                                   {
                                       Group = x.CategoryGroup,
                                       Name = x.CategoryName,
                                       Visible = true
                                   };
            var genderCategories = from x in expected
                                   select new Category
                                   {
                                       Group = CategoryGroup.Gender,
                                       Name = x.Gender,
                                       Visible = true
                                   };
            var categories = filterCategories.Union(genderCategories);

            var profileStore = Substitute.For<IProfileStore>();
            var linkStore = Substitute.For<ICategoryLinkStore>();
            var cache = Substitute.For<ICacheManager>();
            var query = Substitute.For<ICategoryQuery>();

            var sut = new ProfileSearchQuery(profileStore, linkStore, cache, query);

            using (var tokenSource = new CancellationTokenSource())
            {
                query.GetCategories(ReadType.VisibleOnly, tokenSource.Token).Returns(categories);
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
            var query = Substitute.For<ICategoryQuery>();

            Action action = () => new ProfileSearchQuery(profileStore, linkStore, null, query);

            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullLinkStoreTest()
        {
            var profileStore = Substitute.For<IProfileStore>();
            var cache = Substitute.For<ICacheManager>();
            var query = Substitute.For<ICategoryQuery>();

            Action action = () => new ProfileSearchQuery(profileStore, null, cache, query);

            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullProfileStoreTest()
        {
            var linkStore = Substitute.For<ICategoryLinkStore>();
            var cache = Substitute.For<ICacheManager>();
            var query = Substitute.For<ICategoryQuery>();

            Action action = () => new ProfileSearchQuery(null, linkStore, cache, query);

            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullQueryTest()
        {
            var profileStore = Substitute.For<IProfileStore>();
            var linkStore = Substitute.For<ICategoryLinkStore>();
            var cache = Substitute.For<ICacheManager>();

            Action action = () => new ProfileSearchQuery(profileStore, linkStore, cache, null);

            action.ShouldThrow<ArgumentNullException>();
        }
    }
}