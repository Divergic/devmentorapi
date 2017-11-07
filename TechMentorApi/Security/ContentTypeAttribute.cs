namespace TechMentorApi.Security
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using Microsoft.AspNetCore.Http;
    using TechMentorApi.Properties;

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
    public sealed class ContentTypeAttribute : ValidationAttribute
    {
        private static readonly List<string> _supportedContentTypes =
            new List<string> {"image/jpeg", "image/png", "image/gif"};

        public override string FormatErrorMessage(string name)
        {
            return Resources.ContentTypeAttribute_Message;
        }

        public override bool IsValid(object value)
        {
            var file = value as IFormFile;

            if (file == null)
            {
                // This is not the right type to validate
                return false;
            }

            if (_supportedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
            {
                // This is a supported type
                return true;
            }

            return false;
        }
    }
}