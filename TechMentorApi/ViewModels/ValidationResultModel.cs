namespace TechMentorApi.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.AspNetCore.Mvc.ModelBinding;

    /// <summary>
    ///     The <see cref="ValidationResultModel" />
    ///     class describes a validation failure result.
    /// </summary>
    public class ValidationResultModel
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ValidationResultModel" /> class.
        /// </summary>
        /// <param name="modelState">The model state failures.</param>
        public ValidationResultModel(ModelStateDictionary modelState)
        {
            if (modelState == null)
            {
                throw new ArgumentNullException(nameof(modelState));
            }

            Message = "Validation Failed";
            Errors = modelState.Keys.SelectMany(
                key => modelState[key].Errors.Select(x => new ValidationError(key, x.ErrorMessage))).ToList();
        }

        /// <summary>
        ///     Gets or sets the validation errors.
        /// </summary>
        public List<ValidationError> Errors { get; }

        /// <summary>
        ///     Gets or sets the validation message.
        /// </summary>
        public string Message { get; }
    }
}