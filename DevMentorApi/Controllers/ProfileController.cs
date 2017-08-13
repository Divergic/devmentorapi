namespace DevMentorApi.Controllers
{
    using System;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Business;
    using Core;
    using EnsureThat;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Model;
    using Properties;
    using Security;
    using Swashbuckle.AspNetCore.SwaggerGen;

    public class ProfileController : Controller
    {
        private readonly IProfileManager _manager;

        public ProfileController(IProfileManager manager)
        {
            Ensure.That(manager, nameof(manager)).IsNotNull();

            _manager = manager;
        }

        /// <summary>
        ///     Bans the profile by its identifier.
        /// </summary>
        /// <param name="profileId">
        ///     The profile identifier.
        /// </param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        ///     The profile.
        /// </returns>
        [Route("profiles/{profileId:guid}")]
        [HttpDelete]
        [Authorize(Policy = Role.Administrator)]
        [SwaggerResponse((int) HttpStatusCode.NoContent)]
        public async Task<IActionResult> Delete(Guid profileId, CancellationToken cancellationToken)
        {
            if (profileId == Guid.Empty)
            {
                return new ErrorMessageResult(Resources.NotFound, HttpStatusCode.NotFound);
            }

            var bannedAt = DateTimeOffset.UtcNow;

            var profile = await _manager.BanProfile(profileId, bannedAt, cancellationToken).ConfigureAwait(false);

            if (profile == null)
            {
                return new ErrorMessageResult(Resources.NotFound, HttpStatusCode.NotFound);
            }

            return new NoContentResult();
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
        [ProducesResponseType(typeof(PublicProfile), (int) HttpStatusCode.OK)]
        [SwaggerResponse((int) HttpStatusCode.OK, typeof(PublicProfile))]
        [SwaggerResponse((int) HttpStatusCode.NotFound, null, "The profile does not exist.")]
        public async Task<IActionResult> Get(Guid profileId, CancellationToken cancellationToken)
        {
            if (profileId == Guid.Empty)
            {
                return new ErrorMessageResult(Resources.NotFound, HttpStatusCode.NotFound);
            }

            var profile = await _manager.GetPublicProfile(profileId, cancellationToken).ConfigureAwait(false);

            if (profile == null)
            {
                return new ErrorMessageResult(Resources.NotFound, HttpStatusCode.NotFound);
            }

            return new OkObjectResult(profile);
        }
    }
}