namespace TechMentorApi.Controllers
{
    using System;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Swashbuckle.AspNetCore.SwaggerGen;
    using TechMentorApi.Business.Queries;
    using TechMentorApi.Core;
    using TechMentorApi.Properties;

    public class AvatarController
    {
        private readonly IAvatarQuery _query;

        public AvatarController(IAvatarQuery query)
        {
            Ensure.That(query, nameof(query)).IsNotNull();

            _query = query;
        }

        /// <summary>
        ///     Gets the profile avatar by its identifier.
        /// </summary>
        /// <param name="profileId">
        ///     The profile identifier.
        /// </param>
        /// <param name="avatarId">
        ///     The avatar identifier.
        /// </param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        ///     The avatar.
        /// </returns>
        [Route("profiles/{profileId:guid}/avatars/{avatarId:guid}")]
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType((int) HttpStatusCode.OK)]
        [SwaggerResponse((int) HttpStatusCode.OK)]
        [SwaggerResponse((int) HttpStatusCode.NotFound, null, "The avatar does not exist.")]
        public async Task<IActionResult> Get(Guid profileId, Guid avatarId, CancellationToken cancellationToken)
        {
            if (profileId == Guid.Empty)
            {
                return new ErrorMessageResult(Resources.NotFound, HttpStatusCode.NotFound);
            }

            if (avatarId == Guid.Empty)
            {
                return new ErrorMessageResult(Resources.NotFound, HttpStatusCode.NotFound);
            }

            var avatar = await _query.GetAvatar(profileId, avatarId, cancellationToken).ConfigureAwait(false);

            if (avatar == null)
            {
                return new ErrorMessageResult(Resources.NotFound, HttpStatusCode.NotFound);
            }

            return new FileStreamResult(avatar.Data, avatar.ContentType);
        }
    }
}