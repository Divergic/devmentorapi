﻿namespace DevMentorApi.Security
{
    using System.Net;
    using DevMentorApi.Core;
    using DevMentorApi.Properties;
    using Microsoft.AspNetCore.Mvc.Filters;

    public class RequireHttpsFilter : IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {
            // do something after the action executes
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.HttpContext.Request.IsHttps)
            {
                return;
            }

            // do something before the action executes
            context.Result = new ErrorObjectResult(
                Resources.RequireHttpsAttribute_MustUseSsl,
                HttpStatusCode.Forbidden);
        }
    }
}