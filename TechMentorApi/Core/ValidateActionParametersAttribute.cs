namespace TechMentorApi.Core
{
    using System.ComponentModel.DataAnnotations;
    using System.Reflection;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.AspNetCore.Mvc.ModelBinding;

    public class ValidateActionParametersAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var descriptor = context.ActionDescriptor as ControllerActionDescriptor;

            if (descriptor != null)
            {
                var parameters = descriptor.MethodInfo.GetParameters();

                foreach (var parameter in parameters)
                {
                    if (context.ActionArguments.ContainsKey(parameter.Name) == false)
                    {
                        continue;
                    }

                    var argument = context.ActionArguments[parameter.Name];

                    EvaluateValidationAttributes(parameter, argument, context.ModelState);
                }
            }

            base.OnActionExecuting(context);
        }

        private void EvaluateValidationAttributes(ParameterInfo parameter, object argument,
            ModelStateDictionary modelState)
        {
            var validationAttributes = parameter.CustomAttributes;

            foreach (var attributeData in validationAttributes)
            {
                var attributeInstance = parameter.GetCustomAttribute(attributeData.AttributeType);

                var validationAttribute = attributeInstance as ValidationAttribute;

                if (validationAttribute != null)
                {
                    var isValid = validationAttribute.IsValid(argument);
                    if (!isValid)
                    {
                        modelState.AddModelError(parameter.Name,
                            validationAttribute.FormatErrorMessage(parameter.Name));
                    }
                }
            }
        }
    }
}