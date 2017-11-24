namespace TechMentorApi.Controllers
{
    using System;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Swashbuckle.AspNetCore.SwaggerGen;
    using TechMentorApi.Business.Commands;
    using TechMentorApi.Core;
    using TechMentorApi.Model;
    using TechMentorApi.Properties;
    using TechMentorApi.Security;

    public class PhotosController : Controller
    {
        private readonly IPhotoCommand _command;

        public PhotosController(IPhotoCommand command)
        {
            Ensure.Any.IsNotNull(command, nameof(command));

            _command = command;
        }

        /// <summary>
        ///     Creates a new photo.
        /// </summary>
        /// <param name="file">The new photo file.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        ///     A created result.
        /// </returns>
        [Route("profile/photos/")]
        [HttpPost]
        [ProducesResponseType(typeof(PhotoDetails), (int) HttpStatusCode.Created)]
        [SwaggerResponse((int) HttpStatusCode.Created, typeof(PhotoDetails))]
        [SwaggerResponse((int) HttpStatusCode.NotFound)]
        public async Task<IActionResult> Post([ContentType] IFormFile file, CancellationToken cancellationToken)
        {
            if (file == null)
            {
                return new ErrorMessageResult(Resources.Controller_NoBodyDataProvided, HttpStatusCode.BadRequest);
            }

            var profileId = User.Identity.GetClaimValue<Guid>(ClaimType.ProfileId);

            using (var photo = new Photo
            {
                ContentType = file.ContentType,
                Data = file.OpenReadStream(),
                ProfileId = profileId,
                Id = Guid.NewGuid()
            })
            {
                var details = await _command.CreatePhoto(photo, cancellationToken).ConfigureAwait(false);

                var routeValues = new
                {
                    profileId = details.ProfileId,
                    photoId = details.Id
                };

                return new CreatedAtRouteResult("ProfilePhoto", routeValues, details);
            }
        }
    }
}