namespace TechMentorApi.Model
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using TechMentorApi.Model.Properties;

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
    public sealed class RequireBooleanAttribute : ValidationAttribute
    {
        public override string FormatErrorMessage(string name)
        {
            var message = string.Format(CultureInfo.CurrentCulture, Resources.RequireBooleanAttribute_Message, name);

            return message;
        }

        public override bool IsValid(object value)
        {
            if (value == null)
            {
                return false;
            }

            if (value is bool actual)
            {
                if (actual)
                {
                    return true;
                }

                return false;
            }

            return false;
        }
    }
}