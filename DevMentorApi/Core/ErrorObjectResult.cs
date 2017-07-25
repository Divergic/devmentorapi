namespace DevMentorApi.Core
{
    using System.Net;
    using DevMentorApi.Properties;
    using Microsoft.AspNetCore.Mvc;

    public class ErrorObjectResult : ObjectResult
    {
        public ErrorObjectResult(HttpStatusCode statusCode = HttpStatusCode.InternalServerError) : base(
            BuildErrorObject(Resources.WebApi_ExceptionShieldMessage))
        {
            StatusCode = (int)statusCode;
        }

        public ErrorObjectResult(object error, HttpStatusCode statusCode = HttpStatusCode.InternalServerError) : base(DetermineErrorObject(error))
        {
            StatusCode = (int)statusCode;
        }

        public ErrorObjectResult(string message, HttpStatusCode statusCode = HttpStatusCode.InternalServerError) : base(BuildErrorObject(message))
        {
            StatusCode = (int)statusCode;
        }

        private static object BuildErrorObject(string message)
        {
            return new
            {
                Error = message
            };
        }

        private static object DetermineErrorObject(object error)
        {
            if (error == null)
            {
                return BuildErrorObject(Resources.WebApi_ExceptionShieldMessage);
            }

            return error;
        }
    }
}