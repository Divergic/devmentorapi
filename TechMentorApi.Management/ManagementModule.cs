using Autofac;
using System.Net.Http;

namespace TechMentorApi.Management
{
    public class ManagementModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<HttpClient>().As<HttpMessageInvoker>().SingleInstance();
            builder.RegisterType<UserStore>().As<IUserStore>();

            base.Load(builder);
        }
    }
}