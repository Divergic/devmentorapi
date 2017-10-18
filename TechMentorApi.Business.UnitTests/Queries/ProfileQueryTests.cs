namespace TechMentorApi.Business.UnitTests.Queries
{
    using System;
    using System.Collections.ObjectModel;
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

    public class ProfileQueryTests
    {
        [Fact]
        public async Task GetProfileCachesProfileReturnedFromStoreTest()
        {
            var expected = Model.Create<Profile>().Set(x => x.Status = ProfileStatus.Unavailable);

            var store = Substitute.For<IProfileStore>();
            var cache = Substitute.For<ICacheManager>();
            var query = Substitute.For<ICategoryQuery>();

            var sut = new ProfileQuery(store, cache, query);

            using (var tokenSource = new CancellationTokenSource())
            {
                store.GetProfile(expected.Id, tokenSource.Token).Returns(expected);

                var actual = await sut.GetProfile(expected.Id, tokenSource.Token).ConfigureAwait(false);

                cache.Received().StoreProfile(actual);
            }
        }

        [Fact]
        public async Task GetProfileReturnsBannedProfileFromCacheTest()
        {
            var expected = Model.Create<Profile>().Set(x => x.Status = ProfileStatus.Available)
                .Set(x => x.BannedAt = DateTimeOffset.UtcNow);

            var store = Substitute.For<IProfileStore>();
            var cache = Substitute.For<ICacheManager>();
            var query = Substitute.For<ICategoryQuery>();

            var sut = new ProfileQuery(store, cache, query);

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

            var store = Substitute.For<IProfileStore>();
            var cache = Substitute.For<ICacheManager>();
            var query = Substitute.For<ICategoryQuery>();

            var sut = new ProfileQuery(store, cache, query);

            using (var tokenSource = new CancellationTokenSource())
            {
                store.GetProfile(expected.Id, tokenSource.Token).Returns(expected);

                var actual = await sut.GetProfile(expected.Id, tokenSource.Token).ConfigureAwait(false);

                actual.ShouldBeEquivalentTo(expected, opt => opt.ExcludingMissingMembers());
                cache.Received().StoreProfile(expected);
            }
        }

        [Fact]
        public async Task GetProfileReturnsCachedProfileTest()
        {
            var expected = Model.Create<Profile>().Set(x => x.Status = ProfileStatus.Unavailable);

            var store = Substitute.For<IProfileStore>();
            var cache = Substitute.For<ICacheManager>();
            var query = Substitute.For<ICategoryQuery>();

            var sut = new ProfileQuery(store, cache, query);

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

            var store = Substitute.For<IProfileStore>();
            var cache = Substitute.For<ICacheManager>();
            var query = Substitute.For<ICategoryQuery>();

            var sut = new ProfileQuery(store, cache, query);

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

            var store = Substitute.For<IProfileStore>();
            var cache = Substitute.For<ICacheManager>();
            var query = Substitute.For<ICategoryQuery>();

            var sut = new ProfileQuery(store, cache, query);

            using (var tokenSource = new CancellationTokenSource())
            {
                store.GetProfile(expected.Id, tokenSource.Token).Returns(expected);

                var actual = await sut.GetProfile(expected.Id, tokenSource.Token).ConfigureAwait(false);

                actual.ShouldBeEquivalentTo(expected, opt => opt.ExcludingMissingMembers());
                cache.Received().StoreProfile(expected);
            }
        }

        [Fact]
        public async Task GetProfileReturnsNullWhenProfileNotFoundTest()
        {
            var expected = Model.Create<Profile>().Set(x => x.Status = ProfileStatus.Unavailable);

            var store = Substitute.For<IProfileStore>();
            var cache = Substitute.For<ICacheManager>();
            var query = Substitute.For<ICategoryQuery>();

            var sut = new ProfileQuery(store, cache, query);

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
            var query = Substitute.For<ICategoryQuery>();

            var sut = new ProfileQuery(store, cache, query);

            Func<Task> action = async () => await sut.GetProfile(Guid.Empty, CancellationToken.None)
                .ConfigureAwait(false);

            action.ShouldThrow<ArgumentException>();
        }

        [Theory]
        [InlineData(true, "male", "male")]
        [InlineData(true, "male", "Male")]
        [InlineData(true, "Male", "male")]
        [InlineData(false, null, "male")]
        [InlineData(false, "male", "male")]
        [InlineData(false, "male", "Male")]
        [InlineData(false, "Male", "male")]
        public async Task GetPublicProfileAppliesCategoryFilteringToGenderTest(bool isVisible, string profileValue,
            string categoryValue)
        {
            var expected = Model.Create<Profile>().Set(x => x.Status = ProfileStatus.Unavailable)
                .Set(x => x.BannedAt = null).Set(x => x.Gender = profileValue);
            var categories = Model.Create<Collection<Category>>();

            var matchingCategory = new Category
            {
                Group = CategoryGroup.Gender,
                Name = categoryValue,
                Visible = isVisible
            };

            categories.Add(matchingCategory);

            var visibleCategories = categories.Where(x => x.Visible);

            var store = Substitute.For<IProfileStore>();
            var cache = Substitute.For<ICacheManager>();
            var query = Substitute.For<ICategoryQuery>();

            var sut = new ProfileQuery(store, cache, query);

            using (var tokenSource = new CancellationTokenSource())
            {
                store.GetProfile(expected.Id, tokenSource.Token).Returns(expected);
                query.GetCategories(ReadType.VisibleOnly, tokenSource.Token).Returns(visibleCategories);

                var actual = await sut.GetPublicProfile(expected.Id, tokenSource.Token).ConfigureAwait(false);

                if (isVisible)
                {
                    actual.Gender.Should().Be(profileValue);
                }
                else
                {
                    actual.Gender.Should().BeNull();
                }
            }
        }

        [Theory]
        [InlineData(true, "english", "english")]
        [InlineData(true, "english", "English")]
        [InlineData(true, "English", "english")]
        [InlineData(false, "english", "english")]
        [InlineData(false, "english", "English")]
        [InlineData(false, "English", "english")]
        public async Task GetPublicProfileAppliesCategoryFilteringToLanguagesTest(bool isVisible, string profileValue,
            string categoryValue)
        {
            var expected = Model.Create<Profile>().Set(x => x.Status = ProfileStatus.Unavailable)
                .Set(x => x.BannedAt = null).Set(x => x.Languages.Add(profileValue));
            var categories = Model.Create<Collection<Category>>();

            var matchingCategory = new Category
            {
                Group = CategoryGroup.Language,
                Name = categoryValue,
                Visible = isVisible
            };

            categories.Add(matchingCategory);

            var visibleCategories = categories.Where(x => x.Visible);

            var store = Substitute.For<IProfileStore>();
            var cache = Substitute.For<ICacheManager>();
            var query = Substitute.For<ICategoryQuery>();

            var sut = new ProfileQuery(store, cache, query);

            using (var tokenSource = new CancellationTokenSource())
            {
                store.GetProfile(expected.Id, tokenSource.Token).Returns(expected);
                query.GetCategories(ReadType.VisibleOnly, tokenSource.Token).Returns(visibleCategories);

                var actual = await sut.GetPublicProfile(expected.Id, tokenSource.Token).ConfigureAwait(false);

                if (isVisible)
                {
                    actual.Languages.Should().Contain(profileValue);
                }
                else
                {
                    actual.Languages.Should().NotContain(profileValue);
                }
            }
        }

        [Theory]
        [InlineData(true, "azure", "azure")]
        [InlineData(true, "azure", "Azure")]
        [InlineData(true, "Azure", "azure")]
        [InlineData(false, "azure", "azure")]
        [InlineData(false, "azure", "Azure")]
        [InlineData(false, "Azure", "azure")]
        public async Task GetPublicProfileAppliesCategoryFilteringToSkillsTest(bool isVisible, string profileValue,
            string categoryValue)
        {
            var skill = Model.Create<Skill>().Set(x => x.Name = profileValue);
            var expected = Model.Create<Profile>().Set(x => x.Status = ProfileStatus.Unavailable)
                .Set(x => x.BannedAt = null).Set(x => x.Skills.Add(skill));
            var categories = Model.Create<Collection<Category>>();

            var matchingCategory = new Category
            {
                Group = CategoryGroup.Skill,
                Name = categoryValue,
                Visible = isVisible
            };

            categories.Add(matchingCategory);

            var visibleCategories = categories.Where(x => x.Visible);

            var store = Substitute.For<IProfileStore>();
            var cache = Substitute.For<ICacheManager>();
            var query = Substitute.For<ICategoryQuery>();

            var sut = new ProfileQuery(store, cache, query);

            using (var tokenSource = new CancellationTokenSource())
            {
                store.GetProfile(expected.Id, tokenSource.Token).Returns(expected);
                query.GetCategories(ReadType.VisibleOnly, tokenSource.Token).Returns(visibleCategories);

                var actual = await sut.GetPublicProfile(expected.Id, tokenSource.Token).ConfigureAwait(false);

                if (isVisible)
                {
                    actual.Skills.Select(x => x.Name).Should().Contain(profileValue);
                }
                else
                {
                    actual.Skills.Select(x => x.Name).Should().NotContain(profileValue);
                }
            }
        }

        [Fact]
        public async Task GetPublicProfileCachesHiddenProfileReturnedFromStoreTest()
        {
            var expected = Model.Create<Profile>().Set(x => x.Status = ProfileStatus.Hidden)
                .Set(x => x.BannedAt = null);

            var store = Substitute.For<IProfileStore>();
            var cache = Substitute.For<ICacheManager>();
            var query = Substitute.For<ICategoryQuery>();

            var sut = new ProfileQuery(store, cache, query);

            using (var tokenSource = new CancellationTokenSource())
            {
                store.GetProfile(expected.Id, tokenSource.Token).Returns(expected);

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

            var store = Substitute.For<IProfileStore>();
            var cache = Substitute.For<ICacheManager>();
            var query = Substitute.For<ICategoryQuery>();

            var sut = new ProfileQuery(store, cache, query);

            using (var tokenSource = new CancellationTokenSource())
            {
                store.GetProfile(expected.Id, tokenSource.Token).Returns(expected);

                var actual = await sut.GetPublicProfile(expected.Id, tokenSource.Token).ConfigureAwait(false);

                actual.ShouldBeEquivalentTo(expected, opt => opt.ExcludingMissingMembers());
                cache.Received().StoreProfile(expected);
            }
        }

        [Fact]
        public async Task GetPublicProfileHandlesCategoryFilteringWhenGenderIsNullTest()
        {
            var expected = Model.Create<Profile>().Set(x => x.Status = ProfileStatus.Unavailable)
                .Set(x => x.BannedAt = null).Set(x => x.Gender = null);
            var categories = Model.Create<Collection<Category>>();

            var visibleCategories = categories.Where(x => x.Visible);

            var store = Substitute.For<IProfileStore>();
            var cache = Substitute.For<ICacheManager>();
            var query = Substitute.For<ICategoryQuery>();

            var sut = new ProfileQuery(store, cache, query);

            using (var tokenSource = new CancellationTokenSource())
            {
                store.GetProfile(expected.Id, tokenSource.Token).Returns(expected);
                query.GetCategories(ReadType.VisibleOnly, tokenSource.Token).Returns(visibleCategories);

                var actual = await sut.GetPublicProfile(expected.Id, tokenSource.Token).ConfigureAwait(false);

                actual.Gender.Should().BeNullOrEmpty();
            }
        }

        [Fact]
        public async Task GetPublicProfileHandlesCategoryFilteringWhenLanguagesIsNullTest()
        {
            var expected = Model.Create<Profile>().Set(x => x.Status = ProfileStatus.Unavailable)
                .Set(x => x.BannedAt = null).Set(x => x.Languages = null);
            var categories = Model.Create<Collection<Category>>();

            var visibleCategories = categories.Where(x => x.Visible);

            var store = Substitute.For<IProfileStore>();
            var cache = Substitute.For<ICacheManager>();
            var query = Substitute.For<ICategoryQuery>();

            var sut = new ProfileQuery(store, cache, query);

            using (var tokenSource = new CancellationTokenSource())
            {
                store.GetProfile(expected.Id, tokenSource.Token).Returns(expected);
                query.GetCategories(ReadType.VisibleOnly, tokenSource.Token).Returns(visibleCategories);

                var actual = await sut.GetPublicProfile(expected.Id, tokenSource.Token).ConfigureAwait(false);

                actual.Languages.Should().BeNullOrEmpty();
            }
        }

        [Fact]
        public async Task GetPublicProfileHandlesCategoryFilteringWhenSkillsIsNullTest()
        {
            var expected = Model.Create<Profile>().Set(x => x.Status = ProfileStatus.Unavailable)
                .Set(x => x.BannedAt = null).Set(x => x.Skills = null);
            var categories = Model.Create<Collection<Category>>();

            var visibleCategories = categories.Where(x => x.Visible);

            var store = Substitute.For<IProfileStore>();
            var cache = Substitute.For<ICacheManager>();
            var query = Substitute.For<ICategoryQuery>();

            var sut = new ProfileQuery(store, cache, query);

            using (var tokenSource = new CancellationTokenSource())
            {
                store.GetProfile(expected.Id, tokenSource.Token).Returns(expected);
                query.GetCategories(ReadType.VisibleOnly, tokenSource.Token).Returns(visibleCategories);

                var actual = await sut.GetPublicProfile(expected.Id, tokenSource.Token).ConfigureAwait(false);

                actual.Skills.Should().BeNullOrEmpty();
            }
        }

        [Fact]
        public async Task GetPublicProfileReturnsCachedProfileTest()
        {
            var expected = Model.Create<Profile>().Set(x => x.Status = ProfileStatus.Unavailable)
                .Set(x => x.BannedAt = null);

            var store = Substitute.For<IProfileStore>();
            var cache = Substitute.For<ICacheManager>();
            var query = Substitute.For<ICategoryQuery>();

            var sut = new ProfileQuery(store, cache, query);

            using (var tokenSource = new CancellationTokenSource())
            {
                cache.GetProfile(expected.Id).Returns(expected);

                var actual = await sut.GetPublicProfile(expected.Id, tokenSource.Token).ConfigureAwait(false);

                actual.ShouldBeEquivalentTo(expected, opt => opt.ExcludingMissingMembers());
            }
        }

        [Fact]
        public async Task GetPublicProfileReturnsCompleteProfileWhenAllCategoriesApprovedTest()
        {
            var expected = Model.Create<Profile>().Set(x => x.Status = ProfileStatus.Unavailable)
                .Set(x => x.BannedAt = null);
            var categories = new Collection<Category>
            {
                new Category {Group = CategoryGroup.Gender, Name = expected.Gender, Visible = true}
            };

            foreach (var language in expected.Languages)
            {
                categories.Add(new Category {Group = CategoryGroup.Language, Name = language, Visible = true});
            }

            foreach (var skill in expected.Skills)
            {
                categories.Add(new Category {Group = CategoryGroup.Skill, Name = skill.Name, Visible = true});
            }

            var expectedGender = expected.Gender;
            var expectedLanguageCount = expected.Languages.Count;
            var expectedSkillCount = expected.Skills.Count;

            var visibleCategories = categories.Where(x => x.Visible);

            var store = Substitute.For<IProfileStore>();
            var cache = Substitute.For<ICacheManager>();
            var query = Substitute.For<ICategoryQuery>();

            var sut = new ProfileQuery(store, cache, query);

            using (var tokenSource = new CancellationTokenSource())
            {
                store.GetProfile(expected.Id, tokenSource.Token).Returns(expected);
                query.GetCategories(ReadType.VisibleOnly, tokenSource.Token).Returns(visibleCategories);

                var actual = await sut.GetPublicProfile(expected.Id, tokenSource.Token).ConfigureAwait(false);

                actual.Gender.Should().Be(expectedGender);
                actual.Languages.Should().HaveCount(expectedLanguageCount);
                actual.Skills.Should().HaveCount(expectedSkillCount);
            }
        }

        [Fact]
        public async Task GetPublicProfileReturnsNullWhenCachedProfileIsBannedTest()
        {
            var expected = Model.Create<Profile>().Set(x => x.Status = ProfileStatus.Available)
                .Set(x => x.BannedAt = DateTimeOffset.UtcNow);

            var store = Substitute.For<IProfileStore>();
            var cache = Substitute.For<ICacheManager>();
            var query = Substitute.For<ICategoryQuery>();

            var sut = new ProfileQuery(store, cache, query);

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

            var store = Substitute.For<IProfileStore>();
            var cache = Substitute.For<ICacheManager>();
            var query = Substitute.For<ICategoryQuery>();

            var sut = new ProfileQuery(store, cache, query);

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

            var store = Substitute.For<IProfileStore>();
            var cache = Substitute.For<ICacheManager>();
            var query = Substitute.For<ICategoryQuery>();

            var sut = new ProfileQuery(store, cache, query);

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

            var store = Substitute.For<IProfileStore>();
            var cache = Substitute.For<ICacheManager>();
            var query = Substitute.For<ICategoryQuery>();

            var sut = new ProfileQuery(store, cache, query);

            using (var tokenSource = new CancellationTokenSource())
            {
                store.GetProfile(expected.Id, tokenSource.Token).Returns(expected);

                var actual = await sut.GetPublicProfile(expected.Id, tokenSource.Token).ConfigureAwait(false);

                actual.Should().BeNull();
                cache.Received().StoreProfile(expected);
            }
        }

        [Fact]
        public async Task GetPublicProfileReturnsNullWhenStoreProfileIsHiddenTest()
        {
            var expected = Model.Create<Profile>().Set(x => x.Status = ProfileStatus.Hidden);

            var store = Substitute.For<IProfileStore>();
            var cache = Substitute.For<ICacheManager>();
            var query = Substitute.For<ICategoryQuery>();

            var sut = new ProfileQuery(store, cache, query);

            using (var tokenSource = new CancellationTokenSource())
            {
                store.GetProfile(expected.Id, tokenSource.Token).Returns(expected);

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
            var query = Substitute.For<ICategoryQuery>();

            var sut = new ProfileQuery(store, cache, query);

            Func<Task> action = async () => await sut.GetPublicProfile(Guid.Empty, CancellationToken.None)
                .ConfigureAwait(false);

            action.ShouldThrow<ArgumentException>();
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullCacheTest()
        {
            var store = Substitute.For<IProfileStore>();
            var query = Substitute.For<ICategoryQuery>();

            Action action = () => new ProfileQuery(store, null, query);

            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullProfileStoreTest()
        {
            var cache = Substitute.For<ICacheManager>();
            var query = Substitute.For<ICategoryQuery>();

            Action action = () => new ProfileQuery(null, cache, query);

            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullQueryTest()
        {
            var store = Substitute.For<IProfileStore>();
            var cache = Substitute.For<ICacheManager>();

            Action action = () => new ProfileQuery(store, cache, null);

            action.ShouldThrow<ArgumentNullException>();
        }
    }
}