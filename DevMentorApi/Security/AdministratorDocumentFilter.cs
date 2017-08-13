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

                    if (exclude == false)
                    {
                        continue;
                    }

                    var doc = swaggerDoc.Paths["/" + description.RelativePath];

                    if (description.HttpMethod == "GET")
                    {
                        doc.Get = null;
                    }
                    else if (description.HttpMethod == "DELETE")
                    {
                        doc.Delete = null;
                    }
                    else if (description.HttpMethod == "HEAD")
                    {
                        doc.Head = null;
                    }
                    else if (description.HttpMethod == "OPTIONS")
                    {
                        doc.Options = null;
                    }
                    else if (description.HttpMethod == "PATCH")
                    {
                        doc.Patch = null;
                    }
                    else if (description.HttpMethod == "POST")
                    {
                        doc.Post = null;
                    }
                    else if (description.HttpMethod == "PUT")
                    {
                        doc.Put = null;
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