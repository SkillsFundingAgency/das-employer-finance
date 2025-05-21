using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.ApplicationInsights;
using NServiceBus.ObjectBuilder.MSDependencyInjection;
using SFA.DAS.Employer.Shared.UI;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Data;
using SFA.DAS.EmployerFinance.ServiceRegistration;
using SFA.DAS.EmployerFinance.Services;
using SFA.DAS.EmployerFinance.Web.Extensions;
using SFA.DAS.EmployerFinance.Web.Filters;
using SFA.DAS.EmployerFinance.Web.Infrastructure;
using SFA.DAS.EmployerFinance.Web.Middleware;
using SFA.DAS.EmployerFinance.Web.StartupExtensions;
using SFA.DAS.GovUK.Auth.AppStart;
using SFA.DAS.GovUK.Auth.Models;
using SFA.DAS.NServiceBus.Features.ClientOutbox.Data;
using SFA.DAS.UnitOfWork.EntityFrameworkCore.DependencyResolution.Microsoft;
using SFA.DAS.UnitOfWork.Mvc.Extensions;
using SFA.DAS.UnitOfWork.NServiceBus.Features.ClientOutbox.DependencyResolution.Microsoft;
using SFA.DAS.EmployerFinance.Web.Helpers;

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

        services.AddLogging(builder =>
        {
            builder.AddFilter<ApplicationInsightsLoggerProvider>(string.Empty, LogLevel.Information);
            builder.AddFilter<ApplicationInsightsLoggerProvider>("Microsoft", LogLevel.Information);
        });

        services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

        services.AddConfigurationOptions(_configuration);

        var employerFinanceWebConfiguration = _configuration.GetSection(nameof(EmployerFinanceConfiguration)).Get<EmployerFinanceWebConfiguration>();

        services.AddOrchestrators();

        services.AddDatabaseRegistration();
        services.AddDataRepositories();
        services.AddHmrcServices();

        services.AddMaMenuConfiguration(RouteNames.SignOut, _configuration["ResourceEnvironmentName"]);   
            
        //MAC-192
        services.AddApplicationServices(_configuration);

        services.AddCachesRegistrations(_configuration["EnvironmentName"].Equals("LOCAL", StringComparison.CurrentCultureIgnoreCase));

        services.AddContentApiClient(_configuration);
        services.AddEmployerFinanceApi();

        services.AddAuthenticationServices();
       
        services.AddAndConfigureGovUkAuthentication(_configuration, new AuthRedirects
        {
            SignedOutRedirectUrl = "",
            LocalStubLoginPath = "/service/SignIn-Stub"
        }, null, typeof(UserAccountService));

        services.Configure<IISServerOptions>(options => { options.AutomaticAuthentication = false; });

        services.Configure<RouteOptions>(options =>
        {

        }).AddMvc(options =>
        {
            options.Filters.Add(new AnalyticsFilterAttribute());
            if (!_configuration.IsDev())
            {
                options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
                options.Filters.Add(new ZendeskApiFilterAttribute());
            }
        });

        services
            .AddEntityFramework(employerFinanceWebConfiguration)
            .AddEntityFrameworkUnitOfWork<EmployerFinanceDbContext>()
            .AddNServiceBusClientUnitOfWork();
            
        services.AddApplicationInsightsTelemetry(new ApplicationInsightsServiceOptions{ConnectionString = _configuration["APPINSIGHTS_INSTRUMENTATIONKEY"] });
        services.AddLogging(builder =>
        {
            builder.AddFilter<ApplicationInsightsLoggerProvider>(string.Empty, LogLevel.Information);
        });

        if (!_environment.IsDevelopment())
        {
            services.AddDataProtection(_configuration);
        }

        services.AddTransient<IHtmlHelpers, HtmlHelpers>();

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
        
        app.UseMiddleware<SecurityHeadersMiddleware>();

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