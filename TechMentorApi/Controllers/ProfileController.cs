namespace TechMentorApi.Controllers
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
    using TechMentorApi.Business.Commands;
    using TechMentorApi.Business.Queries;

    public class ProfileController : Controller
    {
        private readonly IProfileCommand _command;
        private readonly IProfileQuery _query;

        public ProfileController(IProfileQuery query, IProfileCommand command)
        {
            Ensure.Any.IsNotNull(query, nameof(query));
            Ensure.Any.IsNotNull(command, nameof(command));

            _query = query;
            _command = command;
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

            var profile = await _command.BanProfile(profileId, bannedAt, cancellationToken).ConfigureAwait(false);

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

            var profile = await _query.GetPublicProfile(profileId, cancellationToken).ConfigureAwait(false);

            if (profile == null)
            {
                return new ErrorMessageResult(Resources.NotFound, HttpStatusCode.NotFound);
            }

            return new OkObjectResult(profile);
        }
    }
}