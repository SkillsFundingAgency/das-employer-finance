using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Hosting;
using NServiceBus.ObjectBuilder.MSDependencyInjection;
using SFA.DAS.Employer.Shared.UI;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Data;
using SFA.DAS.EmployerFinance.ServiceRegistration;
using SFA.DAS.EmployerFinance.Web.Extensions;
using SFA.DAS.EmployerFinance.Web.Filters;
using SFA.DAS.EmployerFinance.Web.Handlers;
using SFA.DAS.EmployerFinance.Web.StartupExtensions;
using SFA.DAS.GovUK.Auth.AppStart;
using SFA.DAS.NServiceBus.Features.ClientOutbox.Data;
using SFA.DAS.UnitOfWork.DependencyResolution.Microsoft;
using SFA.DAS.UnitOfWork.EntityFrameworkCore.DependencyResolution.Microsoft;
using SFA.DAS.UnitOfWork.Mvc.Extensions;
using SFA.DAS.UnitOfWork.NServiceBus.Features.ClientOutbox.DependencyResolution.Microsoft;

namespace SFA.DAS.EmployerFinance.Web;

public class Startup
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;
   
    public Startup(IConfiguration configuration, IWebHostEnvironment environment)
    {
        _environment = environment;

        _configuration = configuration.BuildDasConfiguration();
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddHttpContextAccessor();

        services.AddOptions();

        services.AddLogging();

        services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

        services.AddConfigurationOptions(_configuration);

        var employerFinanceConfiguration = _configuration.GetSection(nameof(EmployerFinanceConfiguration)).Get<EmployerFinanceConfiguration>();

        var identityServerConfiguration = _configuration
            .GetSection(nameof(IdentityServerConfiguration))
            .Get<IdentityServerConfiguration>();

        services.AddOrchestrators();

        services.AddDatabaseRegistration(employerFinanceConfiguration.DatabaseConnectionString);
        services.AddDataRepositories();
        services.AddHmrcServices();

        if (employerFinanceConfiguration.UseGovSignIn)
        {
            services.AddMaMenuConfiguration("SignOut", _configuration["ResourceEnvironmentName"]);   
        }
        else
        {
            services.AddMaMenuConfiguration("SignOut", identityServerConfiguration.ClientId, _configuration["ResourceEnvironmentName"]);    
        }
            
        //MAC-192
        services.AddApplicationServices(_configuration);

        services.AddCachesRegistrations(_configuration["EnvironmentName"].Equals("LOCAL", StringComparison.CurrentCultureIgnoreCase));

        services.AddEventsApi();
        //services.AddNotifications(_configuration);
        services.AddEmployerFinanceApi();

        services.AddAuthenticationServices();

        if (_configuration.UseGovUkSignIn())
        {
            services.AddAndConfigureGovUkAuthentication(_configuration,
                typeof(EmployerAccountPostAuthenticationClaimsHandler),
                "",
                "/service/SignIn-Stub");
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
            options.Filters.Add(new AnalyticsFilterAttribute());
            if (!_configuration.IsDev())
            {
                options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
            }

        });

        services.AddNServiceBusClientUnitOfWork();
        services
            .AddUnitOfWork()
            .AddEntityFramework(employerFinanceConfiguration)
            .AddEntityFrameworkUnitOfWork<EmployerFinanceDbContext>();
        services.AddNServiceBusClientUnitOfWork();
            
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
        app.UseUnitOfWork();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
        });
    }
        
    public void ConfigureContainer(UpdateableServiceProvider serviceProvider)
    {
        serviceProvider.StartNServiceBus(_configuration, _configuration.IsDevOrLocal());
        var serviceDescriptor = serviceProvider.FirstOrDefault(serv => serv.ServiceType == typeof(IClientOutboxStorageV2));
        serviceProvider.Remove(serviceDescriptor);
        serviceProvider.AddScoped<IClientOutboxStorageV2, ClientOutboxPersisterV2>();
    }
}