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

    public class PhotoQueryTests
    {
        [Fact]
        public async Task GetPhotoReturnsPhotoFromStoreTest()
        {
            var expected = Model.Ignoring<Photo>(x => x.Data).Create<Photo>();

            var store = Substitute.For<IPhotoStore>();

            var sut = new PhotoQuery(store);

            using (var tokenSource = new CancellationTokenSource())
            {
                store.GetPhoto(expected.ProfileId, expected.Id, tokenSource.Token).Returns(expected);

                var actual = await sut.GetPhoto(expected.ProfileId, expected.Id, tokenSource.Token)
                    .ConfigureAwait(false);

                actual.ShouldBeEquivalentTo(expected);
            }
        }

        [Fact]
        public void ThrowsExceptionWhenCreatedWithNullStoreTest()
        {
            Action action = () => new PhotoQuery(null);

            action.ShouldThrow<ArgumentNullException>();
        }
    }
}