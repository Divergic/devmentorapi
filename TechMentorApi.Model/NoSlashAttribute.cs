using System;
using System.Collections.Generic;
using System.Text;

namespace TechMentorApi.Model
{
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.Text.RegularExpressions;
    using TechMentorApi.Model.Properties;

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
    public class NoSlashAttribute : ValidationAttribute
    {
        private static readonly Regex _expression = new Regex("^[^\\\\/]*$");

        public override string FormatErrorMessage(string name)
        {
            var message = string.Format(CultureInfo.CurrentCulture, Resources.NoSlashAttribute_MessageFormat, name);

            return message;
        }
        
        public override bool IsValid(object value)
        {
            if (value is string singleValue)
            {
                return IsValid(singleValue);
            }

            if (value is IEnumerable<string> multipleValues)
            {
                foreach (var multipleValue in multipleValues)
                {
                    var isValid = IsValid(multipleValue);

                    if (isValid == false)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static bool IsValid(string value)
        {
            return _expression.IsMatch(value);
        }
    }
}
