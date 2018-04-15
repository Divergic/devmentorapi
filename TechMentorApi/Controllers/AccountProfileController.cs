namespace TechMentorApi.Controllers
{
    using EnsureThat;
    using Microsoft.AspNetCore.Mvc;
    using Polly;
    using Swashbuckle.AspNetCore.SwaggerGen;
    using System;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using TechMentorApi.Business.Commands;
    using TechMentorApi.Business.Queries;
    using TechMentorApi.Core;
    using TechMentorApi.Model;
    using TechMentorApi.Properties;

    public class AccountProfileController : Controller
    {
        private readonly IAccountCommand _accountCommand;
        private readonly IProfileCommand _profileCommand;
        private readonly IProfileQuery _profileQuery;

        public AccountProfileController(IProfileQuery profileQuery, IProfileCommand profileCommand, IAccountCommand accountCommand)
        {
            Ensure.Any.IsNotNull(profileQuery, nameof(profileQuery));
            Ensure.Any.IsNotNull(profileCommand, nameof(profileCommand));
            Ensure.Any.IsNotNull(accountCommand, nameof(accountCommand));

            _profileQuery = profileQuery;
            _profileCommand = profileCommand;
            _accountCommand = accountCommand;
        }

        /// <summary>
        /// Deletes the profile.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The a no content response.</returns>
        [Route("profile")]
        [HttpDelete]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [SwaggerResponse((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> Delete(CancellationToken cancellationToken)
        {
            var profileId = User.Identity.GetClaimValue<Guid>(ClaimType.ProfileId);

            await _accountCommand.DeleteAccount(User.Identity.Name, profileId, cancellationToken).ConfigureAwait(false);

            return new NoContentResult();
        }

        /// <summary>
        /// Gets the profile.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The profile.</returns>
        [Route("profile")]
        [HttpGet]
        [ProducesResponseType(typeof(Profile), (int)HttpStatusCode.OK)]
        [SwaggerResponse((int)HttpStatusCode.OK, typeof(Profile))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, null, "The profile does not exist.")]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            var profileId = User.Identity.GetClaimValue<Guid>(ClaimType.ProfileId);

            // See Issue 35 - https://github.com/Divergic/techmentorapi/issues/35 The problem is that
            // the account and profile are stored separately This can cause a race condition if
            // multiple concurrent authenticated calls are made to the API for a new account. The
            // first call writes the account, then the profile. The second call will find the account
            // exists, but the profile may have not been created yet causing a 404 We need a retry
            // attempt here to ensure try to recover from this scenario This can be removed in the
            // future if the storage mechanism is changed to one that supports atomic transactions
            var profile = await Policy.HandleResult<Profile>(x => x == null)
                .WaitAndRetryAsync(3,
                    x => TimeSpan.FromMilliseconds(x * 500))
                .ExecuteAsync(() => _profileQuery.GetProfile(profileId, cancellationToken)).ConfigureAwait(false);

            if (profile == null)
            {
                return new ErrorMessageResult(Resources.NotFound, HttpStatusCode.NotFound);
            }

            return new OkObjectResult(profile);
        }

        /// <summary>
        /// Updates the profile.
        /// </summary>
        /// <param name="model">The updated profile.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        [Route("profile")]
        [HttpPut]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [SwaggerResponse((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> Put([FromBody] UpdatableProfile model, CancellationToken cancellationToken)
        {
            if (model == null)
            {
                return new ErrorMessageResult(Resources.Controller_NoBodyDataProvided, HttpStatusCode.BadRequest);
            }

            var profileId = User.Identity.GetClaimValue<Guid>(ClaimType.ProfileId);

            await _profileCommand.UpdateProfile(profileId, model, cancellationToken).ConfigureAwait(false);

            return new NoContentResult();
        }
    }
}