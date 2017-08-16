namespace DevMentorApi.Controllers
{
    using System.Collections.Generic;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using DevMentorApi.Business;
    using DevMentorApi.Model;
    using EnsureThat;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Swashbuckle.AspNetCore.SwaggerGen;

    public class ProfilesController : Controller
    {
        private readonly IProfileSearchManager _manager;

        public ProfilesController(IProfileSearchManager manager)
        {
            Ensure.That(manager, nameof(manager)).IsNotNull();

            _manager = manager;
        }

        /// <summary>
        ///     Gets the profiles that match provided filters.
        /// </summary>
        /// <param name="filters">
        ///     The profile filters.
        /// </param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        ///     The profiles that match the filters.
        /// </returns>
        [Route("profiles")]
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<ProfileResult>), (int)HttpStatusCode.OK)]
        [SwaggerResponse((int)HttpStatusCode.OK, typeof(IEnumerable<ProfileResult>))]
        public async Task<IActionResult> Get(
            IEnumerable<ProfileResultFilter> filters,
            CancellationToken cancellationToken)
        {
            var results = await _manager.GetProfileResults(filters, cancellationToken).ConfigureAwait(false);

            return Ok(results);
        }
    }
}