namespace TechMentorApi.Security
{
    using System.Linq;
    using Microsoft.AspNetCore.Authorization;
    using Swashbuckle.AspNetCore.Swagger;
    using Swashbuckle.AspNetCore.SwaggerGen;

    public class OAuth2OperationFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            var anonymousControllerAttributes = context.ApiDescription.ControllerAttributes().OfType<AllowAnonymousAttribute>();
            var anonymousActionAttributes = context.ApiDescription.ActionAttributes().OfType<AllowAnonymousAttribute>();
            var anonymousAttributes = anonymousControllerAttributes.Union(anonymousActionAttributes);

            if (anonymousAttributes.Any())
            {
                // This action allows anonymous requests
                return;
            }

            operation.Responses.Add(
                "401",
                new Response
                {
                    Description = "Unauthorized"
                });

            var authorizeControllerAttributes = context.ApiDescription.ControllerAttributes().OfType<AuthorizeAttribute>();
            var authorizeActionAttributes = context.ApiDescription.ActionAttributes().OfType<AuthorizeAttribute>();
            var authorizeAttributes = authorizeControllerAttributes.Union(authorizeActionAttributes);

            if (authorizeAttributes.Any(x => string.IsNullOrWhiteSpace(x.Roles) == false))
            {
                // This action requires a role so Forbidden is also a possibility
                operation.Responses.Add(
                    "403",
                    new Response
                    {
                        Description = "Forbidden"
                    });
            }

            //operation.Security = new List<IDictionary<string, IEnumerable<string>>>();
            //operation.Security.Add(
            //    new Dictionary<string, IEnumerable<string>>
            //    {
            //        {
            //            "oauth2", requiredClaimTypes
            //        }
            //    });
        }
    }
}