namespace DevMentorApi.Controllers
{
    using System;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using DevMentorApi.Business;
    using DevMentorApi.Core;
    using DevMentorApi.Model;
    using DevMentorApi.Properties;
    using EnsureThat;
    using Microsoft.AspNetCore.Mvc;
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
        ///     Gets the profile by its identifier.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        ///     The profile.
        /// </returns>
        [Route("profile")]
        [HttpGet]
        [ProducesResponseType(typeof(Profile), (int)HttpStatusCode.OK)]
        [SwaggerResponse((int)HttpStatusCode.OK, typeof(Profile))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, null, "The profile does not exist.")]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            var accountId = User.Identity.GetClaimValue<Guid>(ClaimType.AccountId);

            var profile = await _manager.GetProfile(accountId, cancellationToken).ConfigureAwait(false);

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
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [SwaggerResponse((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> Put([FromBody] Profile model, CancellationToken cancellationToken)
        {
            if (model == null)
            {
                return new ErrorMessageResult(Resources.Controller_NoBodyDataProvided, HttpStatusCode.BadRequest);
            }

            var accountId = User.Identity.GetClaimValue<Guid>(ClaimType.AccountId);

            if (accountId != model.AccountId)
            {
                return new ForbidResult();
            }

            await _manager.UpdateProfile(model, cancellationToken).ConfigureAwait(false);

            return new NoContentResult();
        }
    }
}