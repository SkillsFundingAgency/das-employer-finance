using System.Collections.Generic;
using System.IO;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using NServiceBus.ObjectBuilder.MSDependencyInjection;
using SFA.DAS.Api.Common.Infrastructure;
using SFA.DAS.Authorization.Mvc.Extensions;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.EmployerFinance.Api.Authentication;
using SFA.DAS.EmployerFinance.Api.Authorization;
using SFA.DAS.EmployerFinance.Api.ErrorHandler;
using SFA.DAS.EmployerFinance.Api.Filters;
using SFA.DAS.EmployerFinance.Api.ServiceRegistrations;
using SFA.DAS.EmployerFinance.Authorisation;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Queries.GetPayeSchemeByRef;
using SFA.DAS.EmployerFinance.ServiceRegistration;
using SFA.DAS.UnitOfWork.NServiceBus.Features.ClientOutbox.DependencyResolution.Microsoft;
using SFA.DAS.Validation.Mvc.Extensions;

namespace SFA.DAS.EmployerFinance.Api
{
    public class Startup
    {
        private readonly IConfiguration _configuration;
        private readonly IHostEnvironment _environment;
        public Startup(IConfiguration configuration,IHostEnvironment environment)
        {
            _environment= environment;

            var config = new ConfigurationBuilder()
                .AddConfiguration(configuration)
                .SetBasePath(Directory.GetCurrentDirectory());

#if DEBUG
            if (!_configuration.IsDev())
            {
                config.AddJsonFile("appsettings.json", false)
                    .AddJsonFile("appsettings.Development.json", true);
            }
#endif

            config.AddEnvironmentVariables();

            if (!configuration.IsTest())
            {
                config.AddAzureTableStorage(options =>
                    {
                        options.ConfigurationKeys = configuration["ConfigNames"].Split(",");
                        options.StorageConnectionString = configuration["ConfigurationStorageConnectionString"];
                        options.EnvironmentName = configuration["EnvironmentName"];
                        options.PreFixConfigurationKeys = false;
                    }
                );
            }
            _configuration = config.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var employerFinanceConfiguration = _configuration.Get<EmployerFinanceConfiguration>();
            var isDevelopment = _configuration.IsDevOrLocal();

            services.AddApiAuthentication(_configuration, isDevelopment);
            services.AddApiAuthorization(isDevelopment);

            services.AddSwaggerGen(c =>
            {
                c.OperationFilter<AuthorizationHeaderParameterOperationFilter>();
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Employer Finance API"
                });
            });

            services.AddApplicationServices();
            services.AddDasDistributedMemoryCache(employerFinanceConfiguration,isDevelopment);
      
            services.AddOrchestrators();

            //services.AddEntityFrameworkUnitOfWork<EmployerFinanceDbContext>();
            services.AddNServiceBusClientUnitOfWork();

            services.AddDatabaseRegistration(employerFinanceConfiguration.DatabaseConnectionString);
            services.AddDataRepositories();
            services.AddEventsApi();


            services.AddMediatorValidators();
            services.AddMediatR(typeof(GetPayeSchemeByRefQuery));
            services.AddNotifications(_configuration);

            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddSingleton<IAuthenticationServiceWrapper, AuthenticationServiceWrapper>();


            services.AddApiConfigurationSections(_configuration)
                .Configure<ApiBehaviorOptions>(opt => { opt.SuppressModelStateInvalidFilter = true; })
                .AddMvc(opt =>
                {
                    if (!_configuration.IsDevOrLocal() && !_configuration.IsTest())
                    {
                        opt.Conventions.Add(new AuthorizeControllerModelConvention(new List<string>()));
                        opt.AddAuthorization();
                    }

                    opt.AddValidation();

                    opt.Filters.Add<StopwatchFilter>();
                });

            //services.AddApplicationInsightsTelemetry();
        }

        public void ConfigureContainer(UpdateableServiceProvider serviceProvider)
        {
            serviceProvider.StartNServiceBus(_configuration, _configuration.IsDevOrLocal() || _configuration.IsTest());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection()
                .UseApiGlobalExceptionHandler(loggerFactory.CreateLogger("Startup"))
                .UseStaticFiles()
                .UseRouting()
                .UseAuthentication()
                .UseAuthorization()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                })
                .UseSwagger()
                .UseSwaggerUI(opt =>
                {
                    opt.SwaggerEndpoint("/swagger/v1/swagger.json", "Employer Finance API");
                    opt.RoutePrefix = "swagger";
                }
                );
        }
    }
}
