namespace TechMentorApi.UnitTests.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.AspNetCore.Mvc;
    using ModelBuilder;
    using NSubstitute;
    using TechMentorApi.Business.Queries;
    using TechMentorApi.Controllers;
    using TechMentorApi.Model;
    using Xunit;

    public class ProfilesControllerTests
    {
        [Fact]
        public async Task GetReturnsResultsFromManagerTest()
        {
            var expected = Model.Create<List<ProfileResult>>();
            var filters = Model.Create<List<ProfileFilter>>();

            var manager = Substitute.For<IProfileSearchQuery>();

            var sut = new ProfilesController(manager);

            using (var tokenSource = new CancellationTokenSource())
            {
                manager.GetProfileResults(filters, tokenSource.Token).Returns(expected);

                var actual = await sut.Get(filters, tokenSource.Token).ConfigureAwait(false);

                actual.Should().BeOfType<OkObjectResult>();

                var result = actual.As<OkObjectResult>();

                result.Value.Should().BeEquivalentTo(expected);
            }
        }

        [Fact]
        public async Task GetReturnsResultsWithExpectedSortOrderTest()
        {
            var source = Model.Create<List<ProfileResult>>();
            var expected = (from x in source
                orderby x.Status descending, x.YearStartedInTech ?? 0 descending, x.BirthYear ??
                                                                                  DateTimeOffset.UtcNow.Year
                select x).ToList();

            var filters = new List<ProfileFilter>();

            var manager = Substitute.For<IProfileSearchQuery>();

            var sut = new ProfilesController(manager);

            using (var tokenSource = new CancellationTokenSource())
            {
                manager.GetProfileResults(filters, tokenSource.Token).Returns(source);

                var actual = await sut.Get(filters, tokenSource.Token).ConfigureAwait(false);

                actual.Should().BeOfType<OkObjectResult>();

                var result = actual.As<OkObjectResult>();

                result.Value.As<IEnumerable<ProfileResult>>().Should().ContainInOrder(expected);
            }
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullQueryTest()
        {
            Action action = () => new ProfilesController(null);

            action.Should().Throw<ArgumentNullException>();
        }
    }
}