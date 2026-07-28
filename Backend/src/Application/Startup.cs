using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection;
using System.Text;
using AutoMapper;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OData.Edm;
using Microsoft.OpenApi.Models;
using OData.Swagger.Services;
using IERIC.SumariosIERIC.BuildingBlocks.EventBus;
using IERIC.SumariosIERIC.BuildingBlocks.EventBus.Abstractions;
using IERIC.SumariosIERIC.BuildingBlocks.EventBusRabbitMQ;
using IERIC.SumariosIERIC.BuildingBlocks.IntegrationEventLogEF;
using IERIC.SumariosIERIC.BuildingBlocks.IntegrationEventLogEF.Services;
using IERIC.SumariosIERIC.Application.Exceptions;
using IERIC.SumariosIERIC.Application.Helper;
using
    IERIC.SumariosIERIC.Application.IntegrationEvents;
using IERIC.SumariosIERIC.Application.Middlewares;
using IERIC.SumariosIERIC.Application.Queries;
using IERIC.SumariosIERIC.Domain.Entities;
using IERIC.SumariosIERIC.Domain.Exceptions;
using IERIC.SumariosIERIC.Infrastructure;
using
    IERIC.SumariosIERIC.Infrastructure.Repositories;
using RabbitMQ.Client;
using IERIC.SumariosIERIC.Domain.Events;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Auth;
using Microsoft.AspNetCore.Http;
using System.Security.Principal;
using Newtonsoft.Json.Serialization;
using Microsoft.Extensions.Options;
using Minio;
using IERIC.SumariosIERIC.Domain.Services;
using IERIC.SumariosIERIC.Infrastructure.Services;

namespace IERIC.SumariosIERIC.Application
{
    public class Startup
    {
        private readonly IWebHostEnvironment _env;

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            _env = env;

            var builder =
                new ConfigurationBuilder()
                    .SetBasePath(env.ContentRootPath)
                    .AddEnvironmentVariables();

            if (_env.IsProduction())
            {
                Console.WriteLine("--> Corriendo en Produccion");
                builder
                    .AddJsonFile("appSettings.production.json",
                    optional: false,
                    reloadOnChange: true);
            }
            else
            {
                Console.WriteLine("--> Corriendo en Desarrollo");
                builder
                    .AddJsonFile("appSettings.development.json",
                    optional: false,
                    reloadOnChange: true);
            }
            this.Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<IPrincipal>(
                provider => provider.GetService<IHttpContextAccessor>().HttpContext.User);
            services.AddControllersWithViews().AddNewtonsoftJson();
            //services.AddAuthorization();
            services.AddAutorizacion(Configuration);
            services.AddControllers();
            services
                .AddSwaggerGen(c =>
                {
                    c
                        .SwaggerDoc("v1",
                        new OpenApiInfo
                        {
                            Title = "Sumarios IERIC",
                            Version = "v1"
                        });
                    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                    {
                        Description = @"JWT Authorization header using the Bearer scheme. \r\n\r\n 
                      Enter 'Bearer' [space] and then your token in the text input below.
                      \r\n\r\nExample: 'Bearer 12345abcdef'",
                        Name = "Authorization",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.ApiKey,
                        Scheme = "Bearer"
                    });

                    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                    {
                        {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header,

                            },
                            new List<string>()
                        }
                        });
                });

            services.AddAutoMapper(typeof(Startup));
            services.AddMediatR(Assembly.GetExecutingAssembly());            
            services.AddSingleton<
                IAleatoriedadQuiz,
                AleatoriedadCriptograficaQuiz
            >();

            services.AddSingleton<
                IQuizRepository,
                QuizEnMemoriaRepository
            >();

            services.AddTransient<
                IGeneradorQuiz,
                GeneradorQuiz
            >();
            services.AddHttpClient();
            services.AddOdataSwaggerSupport();

            //Queries
            services.AddQueries(Configuration);

            // Base de Datos
            services.AddDatabaseContext(Configuration);

            //Eventos de Dominio
            services.AddDomainEvents(Configuration);

            //Base de datos de Log
            services.AddIntegartionEventLog(Configuration);

            //minio

            services.AddScoped(typeof(Functions), typeof(Functions));

            MinioConfig minioConfig = Configuration.GetSection("Minio").Get<MinioConfig>();

            services.AddSingleton<MinioConfig>(minioConfig);

            var minio = new MinioClient()
                                          .WithEndpoint(minioConfig.minioEndpoint)
                                          .WithCredentials(minioConfig.minioAccessKey, minioConfig.minioSecretKey)
                                          .Build();

            services.AddSingleton<MinioClient>(minio);



            // Authenticacion
            //services.AddAuthentication(Configuration);

            // Eventos de Integracion
            //services.AddEventBus(Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //configurar custom errors
            ConfigureErrors(app);
            app.UseRouting();
            app
                .UseCors(x =>
                    x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

            app.UseAuthentication();
            app.UseAuthorization();
            app
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });
            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app
                    .UseSwaggerUI(c =>
                        c
                            .SwaggerEndpoint("/swagger/v1/swagger.json",
                            "Sumarios IERIC v1"));
            }

            // Suscribirse a eventos de integacion
            //ConfigureEventBus(app);
        }

        private void ConfigureEventBus(IApplicationBuilder app)
        {
            var eventBus =
                app.ApplicationServices.GetRequiredService<IEventBus>();
            eventBus
                .Subscribe
                <EmpresaModificadaIntegrationEvent,
                    EmpresaModificadaIntegrationEventHandler
                >();
            //eventBus.Subscribe<MaterialCreadoIntegrationEvent, MaterialCreadoIntegrationEventHandler>();
        }

        private void ConfigureErrors(IApplicationBuilder app)
        {
            Dictionary<Type, IResultError> exceptions =
                new Dictionary<Type, IResultError>();
            exceptions
                .Add(typeof(IInvalidException), new InvalidResultError());
            exceptions
                .Add(typeof(IForbiddenException), new ForbiddenResultError());
            exceptions
                .Add(typeof(InvalidOperationException),
                new InvalidResultError());
            exceptions
                .Add(typeof(INotFoundException), new NotFoundResultError());

            app.UseMiddleware<ExceptionMiddleware>(exceptions);
        }

        private IEdmModel GetEdmModel()
        {
            var odataBuilder = new ODataConventionModelBuilder();
            return odataBuilder.GetEdmModel();
        }
    }

    static class CustomExtensionsMethods
    {
        public static IServiceCollection
        AddQueries(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {

            services.AddScoped<IEmpresaQueries, EmpresaQueries>();



            services.AddControllers().AddNewtonsoftJson(options => { options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver(); options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore; });

            return services;
        }

        public static IServiceCollection
        AddDatabaseContext(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            services
                .AddDbContext<SumariosContext>(opt =>
                    opt.ConfigureWarnings(warnings => warnings.Ignore(CoreEventId.NavigationBaseIncludeIgnored))
                        .UseSqlServer(configuration
                            .GetConnectionString("DefaultConnection"),
                        b => b.MigrationsAssembly("Application")));




            services
                 .AddScoped(typeof(IUsuarioRepository),
                  typeof(UsuarioRepository));



            return services;
        }

        public static IServiceCollection
        AddDomainEvents(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {



            services
       .AddTransient(typeof(
           INotificationHandler<EmpresaNuevaRequested>
       ),
        typeof(EmpresaNuevaDomainEventHandler));


            return services;
        }

        public static IServiceCollection
        AddIntegartionEventLog(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            services
                .AddDbContext<IntegrationEventLogContext>(options =>
                {
                    options
                        .UseSqlServer(configuration
                            .GetConnectionString("DefaultConnection"),
                        sqlServerOptionsAction: sqlOptions =>
                        {
                            sqlOptions.MigrationsAssembly("Infrastructure");
                            sqlOptions
                                .EnableRetryOnFailure(maxRetryCount: 15,
                                maxRetryDelay: TimeSpan.FromSeconds(30),
                                errorNumbersToAdd: null);
                        });
                });
            return services;
        }

        public static IServiceCollection
        AddEventBus(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            //services.AddTransient<MaterialCreadoIntegrationEventHandler>();
            services.AddTransient<EmpresaModificadaIntegrationEventHandler>();
            services
                .AddTransient
                <IEmpresaIntegrationEventService,
                    EmpresaIntegrationEventService
                >();

            services
                .AddTransient
                <Func<DbConnection, IIntegrationEventLogService>>(sp =>
                    (DbConnection c) => new IntegrationEventLogService(c));

            services
                .AddSingleton<IRabbitMQPersistentConnection>(sp =>
                {
                    var logger =
                        sp
                            .GetRequiredService
                            <ILogger<DefaultRabbitMQPersistentConnection>>();

                    var factory =
                        new ConnectionFactory()
                        {
                            HostName = configuration["EventBusConnection"],
                            DispatchConsumersAsync = true
                        };

                    if (!string.IsNullOrEmpty(configuration["EventBusUserName"])
                    )
                    {
                        factory.UserName = configuration["EventBusUserName"];
                    }

                    if (!string.IsNullOrEmpty(configuration["EventBusPassword"])
                    )
                    {
                        factory.Password = configuration["EventBusPassword"];
                    }

                    var retryCount = 5;
                    if (
                        !string
                            .IsNullOrEmpty(configuration["EventBusRetryCount"])
                    )
                    {
                        retryCount =
                            int.Parse(configuration["EventBusRetryCount"]);
                    }

                    return new DefaultRabbitMQPersistentConnection(factory,
                        logger,
                        retryCount);
                });
            services
                .AddSingleton<IEventBus, EventBusRabbitMQ>(sp =>
                {
                    var subscriptionClientName =
                        configuration["SubscriptionClientName"];
                    var rabbitMQPersistentConnection =
                        sp.GetRequiredService<IRabbitMQPersistentConnection>();
                    ILifetimeScope iLifetimeScope =
                        sp.GetRequiredService<ILifetimeScope>();
                    var logger =
                        sp.GetRequiredService<ILogger<EventBusRabbitMQ>>();
                    var eventBusSubcriptionsManager =
                        sp.GetRequiredService<IEventBusSubscriptionsManager>();

                    var retryCount = 5;
                    if (
                        !string
                            .IsNullOrEmpty(configuration["EventBusRetryCount"])
                    )
                    {
                        retryCount =
                            int.Parse(configuration["EventBusRetryCount"]);
                    }

                    return new EventBusRabbitMQ(rabbitMQPersistentConnection,
                        logger,
                        iLifetimeScope,
                        eventBusSubcriptionsManager,
                        subscriptionClientName,
                        retryCount);
                });

            services
                .AddSingleton
                <IEventBusSubscriptionsManager,
                    InMemoryEventBusSubscriptionsManager
                >();

            return services;
        }
    }
}
