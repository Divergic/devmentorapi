namespace DevMentorApi.Security
{
    using System.Linq;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc.ApiExplorer;
    using Swashbuckle.AspNetCore.Swagger;
    using Swashbuckle.AspNetCore.SwaggerGen;

    public class AdministratorDocumentFilter : IDocumentFilter
    {
        public void Apply(SwaggerDocument swaggerDoc, DocumentFilterContext context)
        {
            foreach (var apiDescriptionsGroup in context.ApiDescriptionsGroups.Items)
            {
                foreach (var description in apiDescriptionsGroup.Items)
                {
                    var exclude = IsExcluded(description);

                    if (exclude)
                    {
                        swaggerDoc.Paths.Remove("/" + description.RelativePath);
                    }
                }
            }
        }

        private static bool IsAdministratorRole(AuthorizeAttribute attribute)
        {
            if (attribute.Policy == Role.Administrator)
            {
                return true;
            }

            if (attribute.Roles == null)
            {
                return false;
            }

            if (attribute.Roles.Contains(Role.Administrator))
            {
                return true;
            }

            return false;
        }

        private static bool IsExcluded(ApiDescription description)
        {
            var controllerExcluded = description.ControllerAttributes().OfType<AuthorizeAttribute>()
                .Any(IsAdministratorRole);

            if (controllerExcluded)
            {
                return true;
            }

            var actionExcluded = description.ActionAttributes().OfType<AuthorizeAttribute>().Any(IsAdministratorRole);

            if (actionExcluded)
            {
                return true;
            }

            return false;
        }
    }
}