namespace TechMentorApi.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using TechMentorApi.Security;

    /// <summary>
    ///     The <see cref="AuthController" />
    ///     class provides access to common authentication logic for derived controllers.
    /// </summary>
    public abstract class AuthController : Controller
    {
        protected bool IsAdministrator()
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