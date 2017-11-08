namespace TechMentorApi.Business.UnitTests.Commands
{
    using System;
    using System.IO;
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
        public async Task CreateAvatarStoresResizedAvatarTest()
        {
            var expected = Model.Ignoring<Avatar>(x => x.Data).Create<Avatar>();
            var details = Model.Create<AvatarDetails>();

            var store = Substitute.For<IAvatarStore>();
            var resizer = Substitute.For<IAvatarResizer>();
            var config = Substitute.For<IAvatarConfig>();
            var resizedAvatar = Substitute.For<Avatar>();

            config.MaxHeight.Returns(Environment.TickCount);
            config.MaxWidth = config.MaxHeight + 1;

            var sut = new AvatarCommand(store, resizer, config);

            using (resizedAvatar)
            {
                resizer.Resize(expected, config.MaxHeight, config.MaxWidth).Returns(resizedAvatar);

                using (var tokenSource = new CancellationTokenSource())
                {
                    store.StoreAvatar(resizedAvatar, tokenSource.Token).Returns(details);

                    var actual = await sut.CreateAvatar(expected, tokenSource.Token).ConfigureAwait(false);

                    actual.ShouldBeEquivalentTo(details);

                    resizedAvatar.Received().Dispose();
                }
            }
        }

        [Fact]
        public void CreateAvatarThrowsExceptionWithNullAvatarTest()
        {
            var store = Substitute.For<IAvatarStore>();
            var resizer = Substitute.For<IAvatarResizer>();
            var config = Substitute.For<IAvatarConfig>();

            var sut = new AvatarCommand(store, resizer, config);

            Func<Task> action = async () => await sut.CreateAvatar(null, CancellationToken.None).ConfigureAwait(false);

            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void ThrowsExceptionWithNullConfigTest()
        {
            var store = Substitute.For<IAvatarStore>();
            var resizer = Substitute.For<IAvatarResizer>();

            Action action = () => new AvatarCommand(store, resizer, null);

            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void ThrowsExceptionWithNullResizerTest()
        {
            var store = Substitute.For<IAvatarStore>();
            var config = Substitute.For<IAvatarConfig>();

            Action action = () => new AvatarCommand(store, null, config);

            action.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void ThrowsExceptionWithNullStoreTest()
        {
            var resizer = Substitute.For<IAvatarResizer>();
            var config = Substitute.For<IAvatarConfig>();

            Action action = () => new AvatarCommand(null, resizer, config);

            action.ShouldThrow<ArgumentNullException>();
        }
    }
}