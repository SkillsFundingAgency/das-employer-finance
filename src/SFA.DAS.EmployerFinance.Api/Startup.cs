using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.ApplicationInsights;
using Microsoft.OpenApi.Models;
using SFA.DAS.Api.Common.Infrastructure;
using SFA.DAS.EmployerFinance.Api.Authentication;
using SFA.DAS.EmployerFinance.Api.Authorization;
using SFA.DAS.EmployerFinance.Api.ErrorHandler;
using SFA.DAS.EmployerFinance.Api.Extensions;
using SFA.DAS.EmployerFinance.Api.Filters;
using SFA.DAS.EmployerFinance.Api.Mappings;
using SFA.DAS.EmployerFinance.Api.Middleware;
using SFA.DAS.EmployerFinance.Api.ServiceRegistrations;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Data;
using SFA.DAS.EmployerFinance.Mappings;
using SFA.DAS.EmployerFinance.Queries.GetPayeSchemeByRef;
using SFA.DAS.EmployerFinance.ServiceRegistration;
using SFA.DAS.Validation.Mvc.Extensions;
using AccountMappings = SFA.DAS.EmployerFinance.Api.Mappings.AccountMappings;

namespace SFA.DAS.EmployerFinance.Api;

public class Startup
{
    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _environment;

    public Startup(IConfiguration configuration, IHostEnvironment environment)
    {
        _environment = environment;
        _configuration = configuration.BuildDasConfiguration();
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddApiConfigurationSections(_configuration);

        var employerFinanceConfiguration = _configuration.GetSection(nameof(EmployerFinanceConfiguration)).Get<EmployerFinanceConfiguration>();
        var isDevelopment = _configuration.IsDevOrLocal();

        services.AddLogging(builder =>
        {
            builder.AddFilter<ApplicationInsightsLoggerProvider>(string.Empty, LogLevel.Information);
            builder.AddFilter<ApplicationInsightsLoggerProvider>("Microsoft", LogLevel.Information);
        });

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
        services.AddDasDistributedMemoryCache(employerFinanceConfiguration, _environment.IsDevelopment());

        services.AddOrchestrators();

        services.AddDatabaseRegistration();
        services.AddDataRepositories();
        services.AddRouting();

        services.AddMediatorValidators();
        services.AddMediatR(x => x.RegisterServicesFromAssembly(typeof(GetPayeSchemeByRefQuery).Assembly));
        services.AddAutoMapper(typeof(Startup).Assembly);
        services.AddAutoMapper(typeof(AccountMappings).Assembly);
        services.AddAutoMapper(typeof(PeriodEndMappings).Assembly);

        services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();


        services.Configure<ApiBehaviorOptions>(opt => { opt.SuppressModelStateInvalidFilter = true; })
            .AddMvc(opt =>
            {
                if (!_configuration.IsDevOrLocal())
                {
                    opt.Conventions.Add(new AuthorizeControllerModelConvention(new List<string>()));
                }

                opt.AddValidation();

                opt.Conventions.Add(new ApiExplorerGroupPerVersionConvention());
            });

        services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
        services.AddScoped<IUrlHelper>(sp =>
        {
            var actionContext = sp.GetService<IActionContextAccessor>().ActionContext;
            return new UrlHelper(actionContext);
        });

        services.AddApplicationInsightsTelemetry();

        if (_configuration["Environment"] != "DEV")
        {
            services.AddHealthChecks()
                .AddDbContextCheck<EmployerFinanceDbContext>();
        }
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseMiddleware<SecurityHeadersMiddleware>();
        
        app.UseApiGlobalExceptionHandler(loggerFactory.CreateLogger("Startup"))
            .UseHealthChecks()
            .UseAuthentication()
            .UseRouting()
            .UseAuthorization()
            .UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
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