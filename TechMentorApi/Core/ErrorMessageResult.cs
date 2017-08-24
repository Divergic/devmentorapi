namespace TechMentorApi.Core
{
    using System.Net;
    using Microsoft.AspNetCore.Mvc;

    public class ErrorMessageResult : ObjectResult
    {
        public ErrorMessageResult(string message, HttpStatusCode statusCode) : base(BuildValue(message))
        {
            StatusCode = (int)statusCode;
        }

        private static object BuildValue(string message)
        {
            return new
            {
                Message = message
            };
        }
    }
}