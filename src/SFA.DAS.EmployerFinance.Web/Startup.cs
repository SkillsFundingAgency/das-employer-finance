using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SFA.DAS.Authentication;
using SFA.DAS.Authorization.Mvc.Extensions;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.ServiceRegistration;
using SFA.DAS.EmployerFinance.Web.StartupExtensions;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace SFA.DAS.EmployerFinance.Web
{
    public class Startup
    {
        private readonly IConfiguration _configuration;
        private EmployerFinanceConfiguration _employerFinanceConfiguration;

        public Startup(IConfiguration configuration)
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
                    }
                );
            }

            _configuration =config.Build();

        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpContextAccessor();

            services.AddOptions();

            services.AddLogging();
            
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddConfigurationOptions(_configuration);

            _employerFinanceConfiguration = _configuration.Get<EmployerFinanceConfiguration>();

            var identityServerConfiguration = _configuration
             .GetSection("Identity")
             .Get<IdentityServerConfiguration>();

            services.AddOrchestrators();
            services.AddAutoMapper(typeof(Startup).Assembly);

            services.AddDatabaseRegistration(_employerFinanceConfiguration, _configuration["Environment"]);
            services.AddDataRepositories();
            
            //MAC-192
            services.AddApplicationServices(_employerFinanceConfiguration);

            services.AddHashingServices(_employerFinanceConfiguration);
            services.AddCachesRegistrations();
            services.AddDateTimeServices(_configuration);
            services.AddEventsApi();

            services.AddEmployerFinanceApi();

#if DEBUG
            services.AddControllersWithViews(o =>
            {
                o.AddAuthorization();
            }).AddRazorRuntimeCompilation();
#endif

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseUnauthorizedAccessExceptionHandler();

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
