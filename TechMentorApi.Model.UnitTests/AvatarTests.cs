namespace TechMentorApi.Model.UnitTests
{
    using FluentAssertions;
    using Xunit;

    public class AvatarTests
    {
        [Theory]
        [InlineData(null, null)]
        [InlineData("", "")]
        [InlineData("8D5212A33BF95D0", "8D5212A33BF95D0")]
        [InlineData("0x8D5212A33BF95D0", "0x8D5212A33BF95D0")]
        [InlineData("\"0x8D5212A33BF95D0\"", "0x8D5212A33BF95D0")]
        [InlineData("\"0x8D52  12A33BF95D0\"", "0x8D5212A33BF95D0")]
        public void SetETagCanSetValueTest(string input, string output)
        {
            var sut = new Avatar();

            sut.SetETag(input);

            var actual = sut.ETag;

            actual.Should().Be(output);
        }
    }
}