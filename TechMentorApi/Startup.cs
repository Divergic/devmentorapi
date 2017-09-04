﻿namespace TechMentorApi
{
    using System;
    using System.Diagnostics;
    using System.IdentityModel.Tokens.Jwt;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using TechMentorApi.Core;
    using TechMentorApi.Security;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.Authorization;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.AzureAppServices;
    using Microsoft.Extensions.PlatformAbstractions;
    using Microsoft.IdentityModel.Tokens;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Swashbuckle.AspNetCore.Swagger;

    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder().SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", false, true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true).AddEnvironmentVariables();

            ConfigurationRoot = builder.Build();
            Configuration = ConfigurationRoot.Get<Config>();
        }

        public void Configure(
            IApplicationBuilder app,
            IHostingEnvironment env,
            ILoggerFactory loggerFactory,
            IApplicationLifetime appLifetime)
        {
            ConfigureLogging(env, loggerFactory);

            var logger = loggerFactory.CreateLogger(typeof(Startup));

            try
            {

                if (env.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();
                }

                app.UseStatusCodePages();

                app.UseMiddleware<ShieldExceptionMiddleware>();

                // This must be second because it calls all subsequent middleware and watches for failures
                app.UseMiddleware<ExceptionMonitorMiddleware>();

                app.UseCors(builder => builder.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());

                // Enable middleware to serve generated Swagger as a JSON endpoint.
                app.UseSwagger();

                // Enable middleware to serve swagger-ui (HTML, JS, CSS etc.), specifying the Swagger JSON endpoint.
                app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "DevMentor API V1"); });

                ConfigureAuthentication(app, env);

                // Convert any Auth0 claims into role claims
                app.UseMiddleware<Auth0ClaimsMiddleware>();

                // Ensure that identity information is populated before MVC middleware executes
                app.UseMiddleware<AccountContextMiddleware>();

                app.UseMvc();

                // If you want to dispose of resources that have been resolved in the
                // application container, register for the "ApplicationStopped" event.
                appLifetime.ApplicationStopped.Register(
                    () =>
                    {
                        ApplicationContainer.Dispose();
                        ApplicationContainer = null;
                    });
            }
            catch (Exception ex)
            {
                var eventId = new EventId(0);

                logger.LogError(eventId, ex, "Failed to configure application.");

                throw;
            }
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            var loggerFactory = services.BuildServiceProvider().GetService<ILoggerFactory>();

            var log = loggerFactory.CreateLogger(typeof(Startup));

            try
            {
                // Add framework services.
                services.AddMvc(
                config =>
                {
                    if (Configuration.Authentication.RequireHttps)
                    {
                        config.Filters.Add(new RequireHttpsFilter());
                    }

                    config.Filters.Add(new AuthorizeFilter("AuthenticatedUser"));
                    config.Filters.Add(new ValidateModelAttribute());

                    // add custom binder to beginning of collection
                    config.ModelBinderProviders.Insert(0, new ProfileFilterModelBinderProvider());
                }).AddJsonOptions(
                options =>
                {
                    options.SerializerSettings.Converters.Add(
                        new StringEnumConverter
                        {
                            CamelCaseText = true
                        });
                    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                });

            services.AddMemoryCache();

            services.AddAuthorization(
                options =>
                {
                    options.AddPolicy("AuthenticatedUser", policyBuilder => policyBuilder.RequireAuthenticatedUser());
                    options.AddPolicy(
                        Role.Administrator,
                        policyBuilder => policyBuilder.RequireRole(Role.Administrator));
                });

            services.AddCors();

            // Register the Swagger generator, defining one or more Swagger documents
            services.AddSwaggerGen(
                c =>
                {
                    c.SwaggerDoc(
                        "v1",
                        new Info
                        {
                            Title = "DevMentor API",
                            Version = "v1"
                        });
                    c.DescribeAllEnumsAsStrings();
                    c.DescribeAllParametersInCamelCase();
                    c.DescribeStringEnumsInCamelCase();

                    //var securityScheme = new OAuth2Scheme
                    //{
                    //    AuthorizationUrl = Configuration.Authentication.AuthorizationUrl,
                    //    Flow = "implicit",
                    //    TokenUrl = Configuration.Authentication.TokenUrl
                    //};

                    //c.AddSecurityDefinition("Auth02", securityScheme);

                    var filePath = Path.Combine(
                        PlatformServices.Default.Application.ApplicationBasePath,
                        "TechMentorApi.xml");
                    c.IncludeXmlComments(filePath);

                    c.OperationFilter<OAuth2OperationFilter>();
                    c.DocumentFilter<AdministratorDocumentFilter>();
                });

                ApplicationContainer = ContainerFactory.Build(services, Configuration);
            }
            catch (TypeInitializationException ex)
            {
                var eventId = new EventId(1);

                log.LogError(eventId, ex, "Failed to configure services");

                var loaderFailure = ex.InnerException as ReflectionTypeLoadException;

                if (loaderFailure != null)
                {
                    foreach (var loaderException in loaderFailure.LoaderExceptions)
                    {
                        log.LogError(eventId, loaderException, "Loader Exception in ConfigureServices");
                    }
                }

                throw;
            }
            catch (Exception ex)
            {
                var eventId = new EventId(2);

                log.LogError(eventId, ex, "Failed to configure services");
            }

            // Create the IServiceProvider based on the container.
            return new AutofacServiceProvider(ApplicationContainer);
        }

        private static byte[] Base64UrlDecode(string arg)
        {
            var s = arg;
            s = s.Replace('-', '+'); // 62nd char of encoding
            s = s.Replace('_', '/'); // 63rd char of encoding
            switch (s.Length % 4) // Pad with trailing '='s
            {
                case 0: break; // No pad chars in this case
                case 2:
                    s += "==";
                    break; // Two pad chars
                case 3:
                    s += "=";
                    break; // One pad char
                default: throw new Exception("Illegal base64url string!");
            }
            return Convert.FromBase64String(s); // Standard base64 decoder
        }

        private void ConfigureAuthentication(IApplicationBuilder app, IHostingEnvironment env)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidIssuer = Configuration.Authentication.Authority,
                ValidAudience = Configuration.Authentication.Audience,

                NameClaimType = "sub",
                RoleClaimType = "role",
                AuthenticationType = "Bearer"
            };

            if (env.IsDevelopment())
            {
                var keyByteArray = Base64UrlDecode(Configuration.Authentication.SecretKey);
                var securityKey = new SymmetricSecurityKey(keyByteArray);

                tokenValidationParameters.ValidateIssuerSigningKey = true;
                tokenValidationParameters.IssuerSigningKey = securityKey;
            }

            var options = new JwtBearerOptions
            {
                TokenValidationParameters = tokenValidationParameters,
                Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        var logFactory = app.ApplicationServices.GetService<ILoggerFactory>();
                        var logger = logFactory.CreateLogger<Startup>();

                        Debug.WriteLine(context.Exception.ToString());

                        logger.LogError(default(EventId), context.Exception, "Authentication failed");

                        return Task.CompletedTask;
                    }
                }
            };

            var tokenHandler = options.SecurityTokenValidators.OfType<JwtSecurityTokenHandler>().FirstOrDefault();

            if (tokenHandler != null)
            {
                // Remove stupid Microsoft claim mappings
                tokenHandler.InboundClaimTypeMap.Clear();
            }

            if (env.IsDevelopment())
            {
                options.RequireHttpsMetadata = false;
            }
            else
            {
                options.Audience = Configuration.Authentication.Audience;
                options.Authority = Configuration.Authentication.Authority;
            }

            // The order in which middleware is configured and added is important
            app.UseJwtBearerAuthentication(options);
        }

        private void ConfigureLogging(IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(ConfigurationRoot.GetSection("Logging"));

            if (env.IsDevelopment())
            {
                loggerFactory.AddDebug();
            }
            else
            {
                loggerFactory.AddAzureWebAppDiagnostics(
                    new AzureAppServicesDiagnosticsSettings
                    {
                        OutputTemplate =
                            "{Timestamp:yyyy-MM-dd HH:mm:ss zzz} [{Level}] {RequestId}-{SourceContext}: {Message}{NewLine}{Exception}"
                    });
            }
        }

        public Config Configuration { get; }

        public IConfigurationRoot ConfigurationRoot { get; }

        private IContainer ApplicationContainer { get; set; }
    }
}