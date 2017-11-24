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

    public class PhotoController : Controller
    {
        private readonly IPhotoQuery _query;

        public PhotoController(IPhotoQuery query)
        {
            Ensure.Any.IsNotNull(query, nameof(query));

            _query = query;
        }

        /// <summary>
        ///     Gets the profile photo by its identifier.
        /// </summary>
        /// <param name="profileId">
        ///     The profile identifier.
        /// </param>
        /// <param name="photoId">
        ///     The photo identifier.
        /// </param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        ///     The photo.
        /// </returns>
        [Route("profiles/{profileId:guid}/photos/{photoId:guid}", Name = "ProfilePhoto")]
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType((int) HttpStatusCode.OK)]
        [SwaggerResponse((int) HttpStatusCode.OK)]
        [SwaggerResponse((int) HttpStatusCode.NotFound, null, "The photo does not exist.")]
        public async Task<IActionResult> Get(Guid profileId, Guid photoId, CancellationToken cancellationToken)
        {
            if (profileId == Guid.Empty)
            {
                return new ErrorMessageResult(Resources.NotFound, HttpStatusCode.NotFound);
            }

            if (photoId == Guid.Empty)
            {
                return new ErrorMessageResult(Resources.NotFound, HttpStatusCode.NotFound);
            }

            var photo = await _query.GetPhoto(profileId, photoId, cancellationToken).ConfigureAwait(false);

            if (photo == null)
            {
                return new ErrorMessageResult(Resources.NotFound, HttpStatusCode.NotFound);
            }

            return new FileStreamResult(photo.Data, photo.ContentType);
        }
    }
}