namespace DevMentorApi.ViewModels
{
    /// <summary>
    ///     The <see cref="ValidationError" />
    ///     class describes a validation error.
    /// </summary>
    public class ValidationError
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ValidationError" /> class.
        /// </summary>
        /// <param name="message">The validation error message.</param>
        public ValidationError(string message)
        {
            Message = message;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ValidationError" /> class.
        /// </summary>
        /// <param name="field">The field that failed validation.</param>
        /// <param name="message">The validation error message.</param>
        public ValidationError(string field, string message)
        {
            Field = field != string.Empty ? field : null;
            Message = message;
        }

        /// <summary>
        ///     Gets or sets the validation field.
        /// </summary>
        public string Field { get; }

        /// <summary>
        ///     Gets or sets the validation message.
        /// </summary>
        public string Message { get; }
    }
}