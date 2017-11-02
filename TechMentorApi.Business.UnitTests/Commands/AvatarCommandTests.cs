namespace TechMentorApi.Business.UnitTests.Commands
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using ModelBuilder;
    using NSubstitute;
    using TechMentorApi.Azure;
    using TechMentorApi.Business.Commands;
    using TechMentorApi.Model;
    using Xunit;

    public class AvatarCommandTests
    {
        [Fact]
        public void ThrowsExceptionWithNullStoreTest()
        {
            Action action = () => new AvatarCommand(null);

            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void CreateAvatarThrowsExceptionWithNullAvatarTest()
        {
            var store = Substitute.For<IAvatarStore>();

            var sut = new AvatarCommand(store);

            Func<Task> action = async () => await sut.CreateAvatar(null, CancellationToken.None).ConfigureAwait(false);

            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public async Task CreateAvatarStoresAvatarTest()
        {
            var expected = Model.Ignoring<Avatar>(x => x.Data).Create<Avatar>();

            var store = Substitute.For<IAvatarStore>();

            var sut = new AvatarCommand(store);

            using (var tokenSource = new CancellationTokenSource())
            {
                store.StoreAvatar(expected, tokenSource.Token).Returns(expected);

                var actual = await sut.CreateAvatar(expected, tokenSource.Token).ConfigureAwait(false);

                actual.ShouldBeEquivalentTo(expected);
            }
        }
    }
}