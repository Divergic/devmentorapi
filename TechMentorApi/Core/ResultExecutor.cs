namespace TechMentorApi.Core
{
    using System;
    using System.Threading.Tasks;
    using TechMentorApi.Properties;
    using EnsureThat;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Infrastructure;

    public class ResultExecutor : IResultExecutor
    {
        private readonly ObjectResultExecutor _oex;

        public ResultExecutor(ObjectResultExecutor oex)
        {
            Ensure.Any.IsNotNull(oex, nameof(oex));
            
            _oex = oex;
        }

        public Task Execute(HttpContext context, ObjectResult result)
        {
            Ensure.Any.IsNotNull(context, nameof(context));
            Ensure.Any.IsNotNull(result, nameof(result));

            if (context.Response.HasStarted)
            {
                throw new InvalidOperationException(Resources.ResultExecutor_ResponseStarted);
            }

            context.Response.Clear();

            if (result.StatusCode != null)
            {
                context.Response.StatusCode = result.StatusCode.Value;
            }

            var actionContext = new ActionContext
            {
                HttpContext = context
            };

            return _oex.ExecuteAsync(actionContext, result);
        }
    }
}