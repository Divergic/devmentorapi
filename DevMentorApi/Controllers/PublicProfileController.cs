namespace DevMentorApi.Controllers
{
    using System;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using DevMentorApi.Business;
    using DevMentorApi.Core;
    using DevMentorApi.Properties;
    using DevMentorApi.ViewModels;
    using EnsureThat;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Swashbuckle.AspNetCore.SwaggerGen;

    public class PublicProfileController : Controller
    {
        private readonly IProfileManager _manager;

        public PublicProfileController(IProfileManager manager)
        {
            Ensure.That(manager, nameof(manager)).IsNotNull();

            _manager = manager;
        }

        /// <summary>
        ///     Gets the profile by its identifier.
        /// </summary>
        /// <param name="profileId">
        ///     The profile identifier.
        /// </param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        ///     The profile.
        /// </returns>
        [Route("profiles/{profileId:guid}")]
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PublicProfile), (int)HttpStatusCode.OK)]
        [SwaggerResponse((int)HttpStatusCode.OK)]
        [SwaggerResponse((int)HttpStatusCode.NotFound, null, "The profile does not exist.")]
        public async Task<IActionResult> Get(Guid profileId, CancellationToken cancellationToken)
        {
            if (profileId == Guid.Empty)
            {
                return new ErrorMessageResult(Resources.NotFound, HttpStatusCode.NotFound);
            }

            var profile = await _manager.GetProfile(profileId, cancellationToken).ConfigureAwait(false);

            if (profile == null)
            {
                return new ErrorMessageResult(Resources.NotFound, HttpStatusCode.NotFound);
            }

            var publicProfile = new PublicProfile(profile);

            return new OkObjectResult(publicProfile);
        }
    }
}