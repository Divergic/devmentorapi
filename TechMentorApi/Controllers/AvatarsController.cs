namespace TechMentorApi.Controllers
{
    using System;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Swashbuckle.AspNetCore.SwaggerGen;
    using TechMentorApi.Business.Commands;
    using TechMentorApi.Model;
    using TechMentorApi.ViewModels;

    public class AvatarsController : Controller
    {
        private readonly IAvatarCommand _command;

        public AvatarsController(IAvatarCommand command)
        {
            Ensure.That(command, nameof(command)).IsNotNull();

            _command = command;
        }

        /// <summary>
        ///     Creates a new avatar.
        /// </summary>
        /// <param name="model">The new avatar.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        ///     A created result.
        /// </returns>
        [Route("profiles/avatars/")]
        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(AvatarDetails), (int) HttpStatusCode.Created)]
        [SwaggerResponse((int) HttpStatusCode.Created, typeof(AvatarDetails))]
        [SwaggerResponse((int) HttpStatusCode.NotFound)]
        public async Task<IActionResult> Post(IFormFile model, CancellationToken cancellationToken)
        {
            var profileId = User.Identity.GetClaimValue<Guid>(ClaimType.ProfileId);

            using (var avatar = new Avatar
            {
                ContentType = model.ContentType,
                Data = model.OpenReadStream(),
                ProfileId = profileId,
                Id = Guid.NewGuid()
            })
            {
                var storedAvatar = await _command.CreateAvatar(avatar, cancellationToken).ConfigureAwait(false);

                var details = new AvatarDetails
                {
                    ETag = storedAvatar.ETag,
                    Id = storedAvatar.Id,
                    ProfileId = storedAvatar.ProfileId
                };

                var routeValues = new
                {
                    profileId = details.ProfileId,
                    avatarId = details.Id
                };

                return new CreatedAtActionResult(nameof(AvatarController.Get), nameof(AvatarController), routeValues, details);
            }
        }
    }
}