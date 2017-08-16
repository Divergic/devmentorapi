namespace DevMentorApi.UnitTests.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using DevMentorApi.Business;
    using DevMentorApi.Controllers;
    using DevMentorApi.Model;
    using FluentAssertions;
    using Microsoft.AspNetCore.Mvc;
    using ModelBuilder;
    using NSubstitute;
    using Xunit;

    public class ProfilesControllerTests
    {
        [Fact]
        public async Task GetReturnsResultsFromManagerTest()
        {
            var expected = Model.Create<List<ProfileResult>>();
            var filters = Model.Create<List<ProfileFilter>>();

            var manager = Substitute.For<IProfileSearchManager>();

            var sut = new ProfilesController(manager);

            using (var tokenSource = new CancellationTokenSource())
            {
                manager.GetProfileResults(filters, tokenSource.Token).Returns(expected);

                var actual = await sut.Get(filters, tokenSource.Token).ConfigureAwait(false);

                actual.Should().BeOfType<OkObjectResult>();

                var result = actual.As<OkObjectResult>();

                result.Value.ShouldBeEquivalentTo(expected);
            }
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullManagerTest()
        {
            Action action = () => new ProfilesController(null);

            action.ShouldThrow<ArgumentNullException>();
        }
    }
}