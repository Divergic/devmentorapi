namespace DevMentorApi.Core
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    public interface IResultExecutor
    {
        Task Execute(HttpContext context, ObjectResult result);
    }
}