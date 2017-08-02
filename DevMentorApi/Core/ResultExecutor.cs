namespace DevMentorApi.Core
{
    using System;
    using System.Threading.Tasks;
    using DevMentorApi.Properties;
    using EnsureThat;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Internal;

    public class ResultExecutor : IResultExecutor
    {
        private readonly ObjectResultExecutor _oex;

        public ResultExecutor(ObjectResultExecutor oex)
        {
            Ensure.That(oex, nameof(oex)).IsNotNull();

            _oex = oex;
        }

        public Task Execute(HttpContext context, ObjectResult result)
        {
            Ensure.That(context, nameof(context)).IsNotNull();
            Ensure.That(result, nameof(result)).IsNotNull();

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