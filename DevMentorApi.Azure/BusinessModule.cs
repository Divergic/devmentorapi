namespace DevMentorApi.Azure
{
    using Autofac;

    public class BusinessModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(ThisAssembly).AsImplementedInterfaces();

            base.Load(builder);
        }
    }
}