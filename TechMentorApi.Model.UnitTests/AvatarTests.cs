namespace TechMentorApi.Model.UnitTests
{
    using System.IO;
    using FluentAssertions;
    using NSubstitute;
    using Xunit;

    public class AvatarTests
    {
        [Fact]
        public void CreatesWithDefaultValues()
        {
            using (var sut = new Avatar())
            {
                sut.ContentType.Should().BeNull();
                sut.Data.Should().BeNull();
                sut.Id.Should().BeEmpty();
                sut.ProfileId.Should().BeEmpty();
                sut.ETag.Should().BeNull();
            }
        }

        [Fact]
        public void DisposeCleansUpDataOnlyOnceTest()
        {
            var data = Substitute.For<Stream>();

            using (var sut = new Avatar())
            {
                sut.Data = data;

                sut.Dispose();
                sut.Dispose();

                data.Received(1).Dispose();
            }
        }

        [Fact]
        public void DisposeCleansUpDataTest()
        {
            var data = Substitute.For<Stream>();

            using (var sut = new Avatar())
            {
                sut.Data = data;

                sut.Dispose();

                data.Received().Dispose();
            }
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData("", "")]
        [InlineData("8D5212A33BF95D0", "8D5212A33BF95D0")]
        [InlineData("0x8D5212A33BF95D0", "0x8D5212A33BF95D0")]
        [InlineData("\"0x8D5212A33BF95D0\"", "0x8D5212A33BF95D0")]
        [InlineData("\"0x8D52  12A33BF95D0\"", "0x8D5212A33BF95D0")]
        public void SetETagCanSetValueTest(string input, string output)
        {
            using (var sut = new Avatar())
            {
                sut.SetETag(input);

                var actual = sut.ETag;

                actual.Should().Be(output);
            }
        }
    }
}