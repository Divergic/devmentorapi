namespace TechMentorApi.UnitTests.Controllers
{
    using System;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.AspNetCore.Mvc;
    using ModelBuilder;
    using NSubstitute;
    using TechMentorApi.Business.Commands;
    using TechMentorApi.Controllers;
    using TechMentorApi.Core;
    using TechMentorApi.Model;
    using TechMentorApi.ViewModels;
    using Xunit;

    public class CategoryControllerTests
    {
        [Fact]
        public async Task PutProvidesCategoryToManagerTest()
        {
            const CategoryGroup group = CategoryGroup.Gender;
            var name = Guid.NewGuid().ToString();
            var model = Model.Create<UpdateCategory>();

            var command = Substitute.For<ICategoryCommand>();

            using (var tokenSource = new CancellationTokenSource())
            {
                using (var target = new CategoryController(command))
                {
                    var actual = await target.Put(group.ToString(), name, model, tokenSource.Token)
                        .ConfigureAwait(false);

                    actual.Should().BeOfType<StatusCodeResult>();

                    var result = actual.As<StatusCodeResult>();

                    result.StatusCode.Should().Be((int) HttpStatusCode.NoContent);

                    await command.Received(1).UpdateCategory(Arg.Any<Category>(), tokenSource.Token)
                        .ConfigureAwait(false);
                    await command.Received().UpdateCategory(Arg.Is<Category>(x => x.Group == group), tokenSource.Token)
                        .ConfigureAwait(false);
                    await command.Received().UpdateCategory(Arg.Is<Category>(x => x.Name == name), tokenSource.Token)
                        .ConfigureAwait(false);
                    await command.Received()
                        .UpdateCategory(Arg.Is<Category>(x => x.Visible == model.Visible), tokenSource.Token)
                        .ConfigureAwait(false);
                    await command.Received()
                        .UpdateCategory(Arg.Is<Category>(x => x.Reviewed == false), tokenSource.Token)
                        .ConfigureAwait(false);
                    await command.Received().UpdateCategory(Arg.Is<Category>(x => x.LinkCount == 0), tokenSource.Token)
                        .ConfigureAwait(false);
                }
            }
        }

        [Theory]
        [InlineData("gender")]
        [InlineData("Gender")]
        [InlineData("GENDER")]
        public async Task PutProvidesCategoryToManagerWithCaseInsensitiveCategoryGroupMatchTest(string group)
        {
            var name = Guid.NewGuid().ToString();
            var model = Model.Create<UpdateCategory>();

            var command = Substitute.For<ICategoryCommand>();

            using (var tokenSource = new CancellationTokenSource())
            {
                using (var target = new CategoryController(command))
                {
                    var actual = await target.Put(group, name, model, tokenSource.Token).ConfigureAwait(false);

                    actual.Should().BeOfType<StatusCodeResult>();

                    var result = actual.As<StatusCodeResult>();

                    result.StatusCode.Should().Be((int) HttpStatusCode.NoContent);

                    await command.Received().UpdateCategory(Arg.Is<Category>(x => x.Group == CategoryGroup.Gender),
                            tokenSource.Token)
                        .ConfigureAwait(false);
                }
            }
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("stuff")]
        [InlineData(" ")]
        public async Task PutReturnsBadRequestWithInvalidGroupTest(string group)
        {
            var name = Guid.NewGuid().ToString();
            var model = Model.Create<UpdateCategory>();

            var command = Substitute.For<ICategoryCommand>();

            using (var target = new CategoryController(command))
            {
                var actual = await target.Put(group, name, model, CancellationToken.None).ConfigureAwait(false);

                actual.Should().BeOfType<NotFoundResult>();
            }
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task PutReturnsBadRequestWithInvalidNameTest(string name)
        {
            const CategoryGroup group = CategoryGroup.Gender;
            var model = Model.Create<UpdateCategory>();

            var command = Substitute.For<ICategoryCommand>();

            using (var target = new CategoryController(command))
            {
                var actual = await target.Put(group.ToString(), name, model, CancellationToken.None)
                    .ConfigureAwait(false);

                actual.Should().BeOfType<NotFoundResult>();
            }
        }

        [Fact]
        public async Task PutReturnsBadRequestWithNoPutDataTest()
        {
            const CategoryGroup group = CategoryGroup.Gender;
            var name = Guid.NewGuid().ToString();

            var command = Substitute.For<ICategoryCommand>();

            using (var target = new CategoryController(command))
            {
                var actual = await target.Put(group.ToString(), name, null, CancellationToken.None)
                    .ConfigureAwait(false);

                actual.Should().BeOfType<ErrorMessageResult>().Which.StatusCode.Should()
                    .Be((int) HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullCommandTest()
        {
            Action action = () => new CategoryController(null);

            action.ShouldThrow<ArgumentNullException>();
        }
    }
}