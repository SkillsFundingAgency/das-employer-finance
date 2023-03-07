using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.Employer.Shared.UI;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.ServiceRegistration;
using SFA.DAS.EmployerFinance.Web.Extensions;
using SFA.DAS.EmployerFinance.Web.Filters;
using SFA.DAS.EmployerFinance.Web.Handlers;
using SFA.DAS.EmployerFinance.Web.StartupExtensions;
using SFA.DAS.GovUK.Auth.AppStart;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace SFA.DAS.EmployerFinance.Web
{
    public class Startup
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;
        private EmployerFinanceConfiguration _employerFinanceConfiguration;

        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            var config = new ConfigurationBuilder()
                .AddConfiguration(configuration)
                .SetBasePath(Directory.GetCurrentDirectory());
#if DEBUG
            if (!configuration.IsDev())
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
                        options.ConfigurationKeysRawJsonResult = new[] { "SFA.DAS.Encoding" };
                    }
                );
            }

            _configuration = config.Build();
            _environment = environment;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            
            services.AddOptions();

            services.AddLogging();
            
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

            services.AddConfigurationOptions(_configuration);

            _employerFinanceConfiguration = _configuration.GetSection(nameof(EmployerFinanceConfiguration)).Get<EmployerFinanceConfiguration>();

            var identityServerConfiguration = _configuration
             .GetSection(nameof(IdentityServerConfiguration))
             .Get<IdentityServerConfiguration>();

            services.AddOrchestrators();
            
            services.AddDatabaseRegistration(_employerFinanceConfiguration.DatabaseConnectionString);
            services.AddDataRepositories();
            services.AddHmrcServices();
            
            services.AddMaMenuConfiguration("SignOut", identityServerConfiguration.ClientId,_configuration["ResourceEnvironmentName"]);
            //MAC-192
            services.AddApplicationServices(_configuration);

            //TODO replace with EncodingService
            services.AddCachesRegistrations(_configuration["EnvironmentName"].Equals("LOCAL", StringComparison.CurrentCultureIgnoreCase));
            
            services.AddEventsApi();
            //services.AddNotifications(_configuration);
            services.AddEmployerFinanceApi();

            services.AddAuthenticationServices();
            
            if (_configuration.UseGovUkSignIn())
            {
                services.AddAndConfigureGovUkAuthentication(_configuration,
                    $"{typeof(Startup).Assembly.GetName().Name}.Auth",
                    typeof(EmployerAccountPostAuthenticationClaimsHandler));
            }
            else
            {
                services.AddAndConfigureEmployerAuthentication(identityServerConfiguration);
            }

            services.Configure<IISServerOptions>(options => { options.AutomaticAuthentication = false; });

            services.Configure<RouteOptions>(options =>
            {

            }).AddMvc(options =>
            {
                options.Filters.Add(new AnalyticsFilter());
                if (!_configuration.IsDev())
                {
                    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
                }

            });

            services.AddApplicationInsightsTelemetry();

            if (!_environment.IsDevelopment())
            {
                services.AddDataProtection();
            }
#if DEBUG
            services.AddControllersWithViews(o => { })
                    .AddRazorRuntimeCompilation();
#endif            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAuthentication();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
