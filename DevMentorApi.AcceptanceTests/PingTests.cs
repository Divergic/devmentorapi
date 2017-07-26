namespace DevMentorApi.AcceptanceTests
{
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Xunit;
    using Xunit.Abstractions;

    public class PingTests
    {
        private readonly ILogger<PingTests> _logger;
        private readonly ITestOutputHelper _output;

        public PingTests(ITestOutputHelper output)
        {
            _output = output;
            _logger = output.BuildLoggerFor<PingTests>();
        }

        [Fact]
        public Task HeadReturnsOkForAnonymousUserTest()
        {
            // This will fail if the response on the ping endpoint is not 200 Ok
            return Client.Head(ApiLocation.Ping, _logger);
        }
    }
}