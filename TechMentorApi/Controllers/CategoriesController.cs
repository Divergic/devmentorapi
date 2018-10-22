namespace TechMentorApi.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using TechMentorApi.Business;
    using TechMentorApi.Business.Commands;
    using TechMentorApi.Business.Queries;
    using TechMentorApi.Core;
    using TechMentorApi.Model;
    using TechMentorApi.Properties;
    using TechMentorApi.Security;
    using TechMentorApi.ViewModels;

    public class CategoriesController : AuthController
    {
        private readonly ICategoryCommand _command;
        private readonly ICategoryQuery _query;

        public CategoriesController(ICategoryQuery query, ICategoryCommand command)
        {
            Ensure.Any.IsNotNull(query, nameof(query));
            Ensure.Any.IsNotNull(command, nameof(command));

            _query = query;
            _command = command;
        }

        /// <summary>
        ///     Gets the available categories.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        ///     The categories.
        /// </returns>
        [Route("categories")]
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<PublicCategory>), (int) HttpStatusCode.OK)]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            var readType = ReadType.VisibleOnly;

            // Check if there is an authenticated user who is an administrator
            // Administrators will receive the full category list with all the properties
            // Non administrators (both anonymous users and authenticated users) will receive 
            // only visible categories with only the public properties being returned
            var isAdministrator = IsAdministrator();

            if (isAdministrator)
            {
                readType = ReadType.All;
            }

            var categories = await _query.GetCategories(readType, cancellationToken).ConfigureAwait(false);

            if (isAdministrator)
            {
                return new OkObjectResult(categories);
            }

            var publicCategories = from x in categories
                select new PublicCategory(x);

            return new OkObjectResult(publicCategories);
        }

        /// <summary>
        ///     Creates a new category.
        /// </summary>
        /// <param name="model">The new category.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        ///     A created result.
        /// </returns>
        [Route("categories")]
        [HttpPost]
        [Authorize(Policy = Role.Administrator)]
        [ProducesResponseType((int) HttpStatusCode.Created)]
        public async Task<IActionResult> Post([FromBody] NewCategory model, CancellationToken cancellationToken)
        {
            if (model == null)
            {
                return new ErrorMessageResult(Resources.Controller_NoBodyDataProvided, HttpStatusCode.BadRequest);
            }

            await _command.CreateCategory(model, cancellationToken).ConfigureAwait(false);

            return new StatusCodeResult((int) HttpStatusCode.Created);
        }
    }
}