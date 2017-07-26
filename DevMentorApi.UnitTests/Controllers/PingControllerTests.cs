namespace DevMentorApi.UnitTests.Controllers
{
    using DevMentorApi.Controllers;
    using FluentAssertions;
    using Microsoft.AspNetCore.Mvc;
    using Xunit;

    public class PingControllerTests
    {
        [Fact]
        public void HeadRetursOkResultTest()
        {
            var sut = new PingController();

            var actual = sut.Head();

            actual.Should().BeOfType<OkResult>();
        }
    }
}