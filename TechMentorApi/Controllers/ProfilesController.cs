namespace TechMentorApi.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using TechMentorApi.Business.Queries;
    using TechMentorApi.Model;

    public class ProfilesController : Controller
    {
        private readonly IProfileSearchQuery _query;

        public ProfilesController(IProfileSearchQuery query)
        {
            Ensure.Any.IsNotNull(query, nameof(query));

            _query = query;
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
        public async Task<IActionResult> Get(IEnumerable<ProfileFilter> filters, CancellationToken cancellationToken)
        {
            var results = await _query.GetProfileResults(filters, cancellationToken).ConfigureAwait(false);

            // Order by available first, highest number of years in tech then oldest by age
            var orderedResults = from x in results
                orderby x.Status descending, x.YearStartedInTech ?? 0 descending, x.BirthYear ?? DateTimeOffset.UtcNow
                                                                                      .Year
                select x;

            return Ok(orderedResults);
        }
    }
}