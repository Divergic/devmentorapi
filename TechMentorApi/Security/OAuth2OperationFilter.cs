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
            var controllerAttributes = context.ApiDescription.ControllerAttributes().OfType<AllowAnonymousAttribute>();
            var actionAttributes = context.ApiDescription.ActionAttributes().OfType<AllowAnonymousAttribute>();
            var attributes = controllerAttributes.Union(actionAttributes);

            if (attributes.Any())
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
            operation.Responses.Add(
                "403",
                new Response
                {
                    Description = "Forbidden"
                });

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