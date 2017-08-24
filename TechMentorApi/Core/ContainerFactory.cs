namespace TechMentorApi.Core
{
    using System.Linq;
    using System.Reflection;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using TechMentorApi.Azure;
    using TechMentorApi.Business;
    using Microsoft.Extensions.DependencyInjection;

    public static class ContainerFactory
    {
        public static IContainer Build(IServiceCollection services, Config configuration)
        {
            // Create the container builder.
            var builder = new ContainerBuilder();

            var moduleAssemblies = new[]
            {
                typeof(ContainerFactory).GetTypeInfo().Assembly,
                typeof(AzureModule).GetTypeInfo().Assembly,
                typeof(BusinessModule).GetTypeInfo().Assembly
            };

            // Register modules
            builder.RegisterAssemblyModules(moduleAssemblies);

            RegisterConfigTypes(builder, configuration);

            builder.Populate(services);

            var container = builder.Build();

            return container;
        }

        private static void RegisterConfigTypes(ContainerBuilder builder, object configuration)
        {
            if (configuration == null)
            {
                return;
            }

            // Register all the properties of the configuration as their interfaces
            // This must be done after registering assembly types and modules because type scanning may have already registered the configuration classes as their interfaces 
            // which means Autofac will return the default classes rather than these configuration instances that have values populated.
            var properties = configuration.GetType().GetTypeInfo().GetProperties();

            foreach (var property in properties)
            {
                var value = property.GetValue(configuration);

                if (value == null)
                {
                    continue;
                }

                var configType = value.GetType();

                if (configType.GetTypeInfo().IsValueType)
                {
                    // Skip value types
                    continue;
                }

                if (configType == typeof(string))
                {
                    // Skip strings which are not value types
                    continue;
                }

                if (configType.GetInterfaces().Any())
                {
                    // This is a type that has interfaces
                    // Assume that it should be registered
                    builder.RegisterInstance(value).AsImplementedInterfaces();
                }

                // Recurse into the child properties
                RegisterConfigTypes(builder, value);
            }
        }
    }
}