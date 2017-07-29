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
        [SwaggerResponse((int)HttpStatusCode.OK)]
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
    }
}