namespace DevMentorApi.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using DevMentorApi.Business;
    using DevMentorApi.Core;
    using DevMentorApi.Model;
    using DevMentorApi.Properties;
    using DevMentorApi.Security;
    using DevMentorApi.ViewModels;
    using EnsureThat;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Swashbuckle.AspNetCore.SwaggerGen;

    public class CategoriesController : Controller
    {
        private readonly ICategoryManager _manager;

        public CategoriesController(ICategoryManager manager)
        {
            Ensure.That(manager, nameof(manager)).IsNotNull();

            _manager = manager;
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
        [ProducesResponseType(typeof(IEnumerable<PublicCategory>), (int)HttpStatusCode.OK)]
        [SwaggerResponse((int)HttpStatusCode.OK, typeof(IEnumerable<PublicCategory>))]
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

            var categories = await _manager.GetCategories(readType, cancellationToken).ConfigureAwait(false);

            if (isAdministrator)
            {
                return new OkObjectResult(categories);
            }

            var publicCategories = from x in categories
                select new PublicCategory(x);

            return new OkObjectResult(publicCategories);
        }

        /// <summary>
        ///     Gets the available categories.
        /// </summary>
        /// <param name="model">The new category.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        ///     The categories.
        /// </returns>
        [Route("categories")]
        [HttpPost]
        [Authorize(Policy = Role.Administrator)]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        [SwaggerResponse((int)HttpStatusCode.Created)]
        public async Task<IActionResult> Post([FromBody] NewCategory model, CancellationToken cancellationToken)
        {
            if (model == null)
            {
                return new ErrorMessageResult(Resources.Controller_NoBodyDataProvided, HttpStatusCode.BadRequest);
            }

            await _manager.CreateCategory(model, cancellationToken).ConfigureAwait(false);

            return new StatusCodeResult((int)HttpStatusCode.Created);
        }

        private bool IsAdministrator()
        {
            if (User == null)
            {
                return false;
            }

            if (User.Identity?.IsAuthenticated == false)
            {
                return false;
            }

            if (User.IsInRole(Role.Administrator))
            {
                return true;
            }

            return false;
        }
    }
}