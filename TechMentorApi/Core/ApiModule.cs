namespace TechMentorApi.Core
{
    using Autofac;
    using SharpRaven;

    public class ApiModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(ThisAssembly).AsImplementedInterfaces();

            builder.Register(x =>
            {
                var config = x.Resolve<ISentryConfig>();

                var client = new RavenClient(config.Dsn)
                {
                    Environment = config.Environment,
                    Release = config.Version
                };

                return client;
            }).SingleInstance().As<IRavenClient>();

            base.Load(builder);
        }
    }
}