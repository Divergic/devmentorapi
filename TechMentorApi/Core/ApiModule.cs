namespace TechMentorApi.Core
{
    using Autofac;

    public class ApiModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(ThisAssembly).AsImplementedInterfaces();

            base.Load(builder);
        }
    }
}