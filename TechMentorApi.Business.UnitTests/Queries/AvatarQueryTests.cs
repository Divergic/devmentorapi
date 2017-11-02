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

    public class AvatarQueryTests
    {
        [Fact]
        public async Task GetAvatarReturnsAvatarFromStoreTest()
        {
            var expected = Model.Ignoring<Avatar>(x => x.Data).Create<Avatar>();

            var store = Substitute.For<IAvatarStore>();

            var sut = new AvatarQuery(store);

            using (var tokenSource = new CancellationTokenSource())
            {
                store.GetAvatar(expected.ProfileId, expected.Id, tokenSource.Token).Returns(expected);

                var actual = await sut.GetAvatar(expected.ProfileId, expected.Id, tokenSource.Token)
                    .ConfigureAwait(false);

                actual.ShouldBeEquivalentTo(expected);
            }
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullStoreTest()
        {
            Action action = () => new AvatarQuery(null);

            action.ShouldThrow<ArgumentNullException>();
        }
    }
}