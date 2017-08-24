namespace TechMentorApi.UnitTests.Core
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using TechMentorApi.Core;
    using FluentAssertions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Internal;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Microsoft.Extensions.Primitives;
    using Model;
    using NSubstitute;
    using Xunit;

    public class ProfileFilterModelBinderTests
    {
        [Fact]
        public async Task BindModelAsyncAddsMatchingFiltersTest()
        {
            var context = new DefaultModelBindingContext();
            var httpContext = Substitute.For<HttpContext>();
            var actionContext = Substitute.For<ActionContext>();
            var request = Substitute.For<HttpRequest>();
            var values = new Dictionary<string, StringValues>
            {
                {"skill", "mySkill"},
                {"language", new StringValues(new[] {"english", "spanish"})},
                {"gender", "female"}
            };
            var query = new QueryCollection(values);

            context.ActionContext = actionContext;
            actionContext.HttpContext = httpContext;
            httpContext.Request.Returns(request);
            request.Query.Returns(query);

            var sut = new ProfileFilterModelBinder();

            await sut.BindModelAsync(context).ConfigureAwait(false);

            context.Result.IsModelSet.Should().BeTrue();

            var actual = context.Result.Model.As<List<ProfileFilter>>();

            actual.Should().HaveCount(4);
            actual.Should().Contain(x => x.CategoryGroup == CategoryGroup.Skill && x.CategoryName == "mySkill");
            actual.Should().Contain(x => x.CategoryGroup == CategoryGroup.Language && x.CategoryName == "english");
            actual.Should().Contain(x => x.CategoryGroup == CategoryGroup.Language && x.CategoryName == "spanish");
            actual.Should().Contain(x => x.CategoryGroup == CategoryGroup.Gender && x.CategoryName == "female");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task BindModelAsyncIgnoresEmptyValuesTest(string value)
        {
            var context = new DefaultModelBindingContext();
            var httpContext = Substitute.For<HttpContext>();
            var actionContext = Substitute.For<ActionContext>();
            var request = Substitute.For<HttpRequest>();
            var values = new Dictionary<string, StringValues> {{"gender", value}};
            var query = new QueryCollection(values);

            context.ActionContext = actionContext;
            actionContext.HttpContext = httpContext;
            httpContext.Request.Returns(request);
            request.Query.Returns(query);

            var sut = new ProfileFilterModelBinder();

            await sut.BindModelAsync(context).ConfigureAwait(false);

            context.Result.IsModelSet.Should().BeTrue();
            context.Result.Model.As<List<ProfileFilter>>().Should().BeEmpty();
        }

        [Fact]
        public async Task BindModelAsyncIgnoresKeysNotMatchingCategoriesTest()
        {
            var context = new DefaultModelBindingContext();
            var httpContext = Substitute.For<HttpContext>();
            var actionContext = Substitute.For<ActionContext>();
            var request = Substitute.For<HttpRequest>();
            var values = new Dictionary<string, StringValues> {{"someKey", "someValue"}};
            var query = new QueryCollection(values);

            context.ActionContext = actionContext;
            actionContext.HttpContext = httpContext;
            httpContext.Request.Returns(request);
            request.Query.Returns(query);

            var sut = new ProfileFilterModelBinder();

            await sut.BindModelAsync(context).ConfigureAwait(false);

            context.Result.IsModelSet.Should().BeTrue();
            context.Result.Model.As<List<ProfileFilter>>().Should().BeEmpty();
        }

        [Theory]
        [InlineData("gender")]
        [InlineData("Gender")]
        [InlineData("GENDER")]
        public async Task BindModelAsyncMatchesCaseInsensitiveCategoryGroupsTest(string key)
        {
            var context = new DefaultModelBindingContext();
            var httpContext = Substitute.For<HttpContext>();
            var actionContext = Substitute.For<ActionContext>();
            var request = Substitute.For<HttpRequest>();
            var values = new Dictionary<string, StringValues> {{key, "female"}};
            var query = new QueryCollection(values);

            context.ActionContext = actionContext;
            actionContext.HttpContext = httpContext;
            httpContext.Request.Returns(request);
            request.Query.Returns(query);

            var sut = new ProfileFilterModelBinder();

            await sut.BindModelAsync(context).ConfigureAwait(false);

            context.Result.IsModelSet.Should().BeTrue();

            var actual = context.Result.Model.As<List<ProfileFilter>>();

            actual.Should().Contain(x => x.CategoryGroup == CategoryGroup.Gender && x.CategoryName == "female");
        }

        [Fact]
        public async Task BindModelAsyncSetsEmptyModelWhenQueryIsEmptyTest()
        {
            var context = new DefaultModelBindingContext();
            var httpContext = Substitute.For<HttpContext>();
            var actionContext = Substitute.For<ActionContext>();
            var request = Substitute.For<HttpRequest>();
            var query = new QueryCollection();

            context.ActionContext = actionContext;
            actionContext.HttpContext = httpContext;
            httpContext.Request.Returns(request);
            request.Query.Returns(query);

            var sut = new ProfileFilterModelBinder();

            await sut.BindModelAsync(context).ConfigureAwait(false);

            context.Result.IsModelSet.Should().BeTrue();
            context.Result.Model.As<List<ProfileFilter>>().Should().BeEmpty();
        }

        [Fact]
        public void BindModelAsyncThrowsExceptionWithNullContextTest()
        {
            var sut = new ProfileFilterModelBinder();

            Func<Task> action = async () => await sut.BindModelAsync(null).ConfigureAwait(false);

            action.ShouldThrow<ArgumentNullException>();
        }
    }
}