namespace DevMentorApi.Core
{
    using System;
    using System.Net.Http;
    using DevMentorApi.ViewModels;
    using EnsureThat;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class ValidateModelAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            Ensure.That(context, nameof(context)).IsNotNull();

            if (context.HttpContext.Request.Method == HttpMethod.Get.ToString())
            {
                return;
            }

            if (context.HttpContext.Request.Method == HttpMethod.Head.ToString())
            {
                return;
            }

            if (context.ModelState.IsValid)
            {
                return;
            }

            var modelError = new ValidationResultModel(context.ModelState);

            context.Result = new BadRequestObjectResult(modelError);
        }
    }
}