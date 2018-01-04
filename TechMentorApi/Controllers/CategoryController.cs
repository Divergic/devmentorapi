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
    using TechMentorApi.Business;
    using TechMentorApi.Business.Commands;
    using TechMentorApi.Business.Queries;
    using TechMentorApi.Core;
    using TechMentorApi.Model;
    using TechMentorApi.Properties;
    using TechMentorApi.Security;
    using TechMentorApi.ViewModels;

    public class CategoryController : AuthController
    {
        private readonly ICategoryCommand _command;
        private readonly ICategoryQuery _query;

        public CategoryController(ICategoryQuery query, ICategoryCommand command)
        {
            Ensure.Any.IsNotNull(command, nameof(command));
            Ensure.Any.IsNotNull(query, nameof(query));

            _command = command;
            _query = query;
        }

        /// <summary>
        ///     Gets the category by its group and name.
        /// </summary>
        /// <param name="group">The group of the category.</param>
        /// <param name="name">The name of the category</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        ///     Ok if the category is found; otherwise NotFound.
        /// </returns>
        [Route("categories/{group:alpha}/{name}")]
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PublicCategory), (int)HttpStatusCode.OK)]
        [SwaggerResponse((int)HttpStatusCode.OK, typeof(PublicCategory))]
        [SwaggerResponse((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> Get(string group, string name, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(group))
            {
                return new ErrorMessageResult("No category group specified.", HttpStatusCode.NotFound);
            }

            if (Enum.TryParse<CategoryGroup>(group, true, out var categoryGroup) == false)
            {
                return new ErrorMessageResult("Category group specified is invalid.", HttpStatusCode.NotFound);
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                return new ErrorMessageResult("No category name specified.", HttpStatusCode.NotFound);
            }

            var readType = ReadType.VisibleOnly;

            // Check if there is an authenticated user who is an administrator
            // Administrators will receive the full category with all the properties
            // Non administrators (both anonymous users and authenticated users) will receive 
            // only a visible category with only the public properties being returned
            var isAdministrator = IsAdministrator();

            if (isAdministrator)
            {
                readType = ReadType.All;
            }

            var category = await _query.GetCategory(readType, categoryGroup, name, cancellationToken)
                .ConfigureAwait(false);

            if (category == null)
            {
                return new ErrorMessageResult("The requested category does not exist.", HttpStatusCode.NotFound);
            }

            if (isAdministrator)
            {
                return new OkObjectResult(category);
            }

            var publicCategory = new PublicCategory(category);

            return new OkObjectResult(publicCategory);
        }

        /// <summary>
        ///     Updates the category's visibility.
        /// </summary>
        /// <param name="group">The group of the category.</param>
        /// <param name="name">The name of the category</param>
        /// <param name="model">The category information.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        ///     NoContent if the call is successful.
        /// </returns>
        [Route("categories/{group:alpha}/{name}")]
        [HttpPut]
        [Authorize(Policy = Role.Administrator)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [SwaggerResponse((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> Put(
            string group,
            string name,
            [FromBody] UpdateCategory model,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(group))
            {
                return new ErrorMessageResult("No category group specified.", HttpStatusCode.NotFound);
            }

            if (Enum.TryParse<CategoryGroup>(group, true, out var categoryGroup) == false)
            {
                return new ErrorMessageResult("Category group specified is invalid.", HttpStatusCode.NotFound);
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                return new ErrorMessageResult("No category name specified.", HttpStatusCode.NotFound);
            }

            if (model == null)
            {
                return new ErrorMessageResult(Resources.Controller_NoBodyDataProvided, HttpStatusCode.BadRequest);
            }

            var category = new Category
            {
                Group = categoryGroup,
                Name = name,
                Visible = model.Visible
            };

            await _command.UpdateCategory(category, cancellationToken).ConfigureAwait(false);

            return new StatusCodeResult((int)HttpStatusCode.NoContent);
        }
    }
}