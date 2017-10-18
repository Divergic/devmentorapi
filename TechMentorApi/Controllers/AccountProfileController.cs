namespace TechMentorApi.Controllers
{
    using System;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.AspNetCore.Mvc;
    using Swashbuckle.AspNetCore.SwaggerGen;
    using TechMentorApi.Business.Commands;
    using TechMentorApi.Business.Queries;
    using TechMentorApi.Core;
    using TechMentorApi.Model;
    using TechMentorApi.Properties;

    public class AccountProfileController : Controller
    {
        private readonly IProfileCommand _command;
        private readonly IProfileQuery _query;

        public AccountProfileController(IProfileQuery query, IProfileCommand command)
        {
            Ensure.That(query, nameof(query)).IsNotNull();
            Ensure.That(command, nameof(command)).IsNotNull();

            _query = query;
            _command = command;
        }

        /// <summary>
        ///     Gets the profile by its identifier.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        ///     The profile.
        /// </returns>
        [Route("profile")]
        [HttpGet]
        [ProducesResponseType(typeof(Profile), (int) HttpStatusCode.OK)]
        [SwaggerResponse((int) HttpStatusCode.OK, typeof(Profile))]
        [SwaggerResponse((int) HttpStatusCode.NotFound, null, "The profile does not exist.")]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            var profileId = User.Identity.GetClaimValue<Guid>(ClaimType.ProfileId);

            var profile = await _query.GetProfile(profileId, cancellationToken).ConfigureAwait(false);

            if (profile == null)
            {
                return new ErrorMessageResult(Resources.NotFound, HttpStatusCode.NotFound);
            }

            return new OkObjectResult(profile);
        }

        /// <summary>
        ///     Updates the profile.
        /// </summary>
        /// <param name="model">The updated profile.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        [Route("profile")]
        [HttpPut]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [SwaggerResponse((int) HttpStatusCode.NoContent)]
        public async Task<IActionResult> Put([FromBody] UpdatableProfile model, CancellationToken cancellationToken)
        {
            if (model == null)
            {
                return new ErrorMessageResult(Resources.Controller_NoBodyDataProvided, HttpStatusCode.BadRequest);
            }

            var profileId = User.Identity.GetClaimValue<Guid>(ClaimType.ProfileId);

            await _command.UpdateProfile(profileId, model, cancellationToken).ConfigureAwait(false);

            return new NoContentResult();
        }
    }
}