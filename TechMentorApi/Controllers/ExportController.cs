using EnsureThat;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using TechMentorApi.Business.Queries;
using TechMentorApi.Core;
using TechMentorApi.Model;
using TechMentorApi.Properties;

namespace TechMentorApi.Controllers
{
    public class ExportController : Controller
    {
        private readonly IExportQuery _query;

        public ExportController(IExportQuery query)
        {
            Ensure.Any.IsNotNull(query, nameof(query));

            _query = query;
        }

        /// <summary>
        ///     Exports the profile data with all currently stored profile photos.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        ///     All stored profile data.
        /// </returns>
        [Route("profile/export/")]
        [HttpGet]
        [ProducesResponseType(typeof(ExportProfile), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            var profileId = User.Identity.GetClaimValue<Guid>(ClaimType.ProfileId);

            var profile = await _query.GetExportProfile(profileId, cancellationToken).ConfigureAwait(false);

            if (profile == null)
            {
                return new ErrorMessageResult(Resources.NotFound, HttpStatusCode.NotFound);
            }

            return new OkObjectResult(profile);
        }
    }
}