namespace DevMentorApi.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    ///     The Ping endpoint is used to determine whether the API is available without causing a performance impact.
    /// </summary>
    [Route("ping")]
    [AllowAnonymous]
    public class PingController : Controller
    {
        /// <summary>
        ///     Returns an Ok response to indicate that the service is running.
        /// </summary>
        /// <returns>An Ok response.</returns>
        [HttpHead]
        public IActionResult Head()
        {
            return new OkResult();
        }
    }
}