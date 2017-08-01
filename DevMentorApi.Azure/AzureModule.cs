namespace DevMentorApi.Azure
{
    using Autofac;

    public class AzureModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(ThisAssembly).AsImplementedInterfaces();

            base.Load(builder);
        }
    }
}