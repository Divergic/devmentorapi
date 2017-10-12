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
    using ViewModels;

    public class CategoryController : Controller
    {
        private readonly ICategoryCommand _command;

        public CategoryController(ICategoryCommand command)
        {
            Ensure.That(command, nameof(command)).IsNotNull();

            _command = command;
        }

        /// <summary>
        ///     Gets the available categories.
        /// </summary>
        /// <param name="group">The group of the category.</param>
        /// <param name="name">The name of the category</param>
        /// <param name="model">The new category.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        ///     The categories.
        /// </returns>
        [Route("categories/{group:alpha}/{name}")]
        [HttpPut]
        [Authorize(Policy = Role.Administrator)]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [SwaggerResponse((int) HttpStatusCode.NoContent)]
        public async Task<IActionResult> Put(string group, string name, [FromBody] UpdateCategory model,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(group))
            {
                return new NotFoundResult();
            }

            if (Enum.TryParse<CategoryGroup>(group, true, out var categoryGroup) == false)
            {
                return new NotFoundResult();
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                return new NotFoundResult();
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

            return new StatusCodeResult((int) HttpStatusCode.NoContent);
        }
    }
}