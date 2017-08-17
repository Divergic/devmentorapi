namespace DevMentorApi.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Business;
    using EnsureThat;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Model;
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
        [ProducesResponseType(typeof(IEnumerable<ProfileResult>), (int) HttpStatusCode.OK)]
        [SwaggerResponse((int) HttpStatusCode.OK, typeof(IEnumerable<ProfileResult>))]
        public async Task<IActionResult> Get(
            IEnumerable<ProfileFilter> filters,
            CancellationToken cancellationToken)
        {
            var results = await _manager.GetProfileResults(filters, cancellationToken).ConfigureAwait(false);

            // Order by available first, highest number of years in tech then oldest by age
            var orderedResults = from x in results
                orderby x.Status descending, x.YearStartedInTech ?? 0 descending, x.BirthYear ??
                                                                                  DateTimeOffset.UtcNow.Year
                select x;

            return Ok(orderedResults);
        }
    }
}