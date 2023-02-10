using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Data;
using SFA.DAS.EmployerFinance.Factories;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Web.Extensions;
using SFA.DAS.EmployerFinance.Web.Helpers;
using SFA.DAS.Encoding;
using SFA.DAS.Hmrc.ExecutionPolicy;
using SFA.DAS.NServiceBus.Services;

namespace SFA.DAS.EmployerFinance.Web.StartupExtensions
{
    public static class ApplicationServiceRegistrations
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, EmployerFinanceConfiguration configuration)
        {
            //MAP-192 Needimplementing

            services.AddScoped<IHtmlHelperExtensions, HtmlHelperExtensions>();
            //services.AddScoped<ActivitiesHelper>();
            //services.AddTransient<IRestClientFactory, RestClientFactory>();
            //services.AddTransient<IRestServiceFactory, RestServiceFactory>();
            //services.AddTransient<IHttpServiceFactory, HttpServiceFactory>();
            //services.AddTransient<IUserAornPayeLockService, UserAornPayeLockService>();

            //services.AddScoped<IProviderRegistrationApiClient, ProviderRegistrationApiClient>();

            //services.AddTransient<IReservationsService, ReservationsService>();
            //services.Decorate<IReservationsService, ReservationsServiceWithTimeout>();

            //services.AddTransient<ICommitmentV2Service, CommitmentsV2Service>();
            //services.Decorate<ICommitmentV2Service, CommitmentsV2ServiceWithTimeout>();

            //services.AddTransient<IRecruitService, RecruitService>();
            //services.Decorate<IRecruitService, RecruitServiceWithTimeout>();

            services.AddScoped<IAccountApiClient, AccountApiClient>();
            //services.AddTransient<IReferenceDataService, ReferenceDataService>();
            //services.AddScoped<ITaskApiClient, TaskApiClient>();
            //services.AddTransient<ITaskService, TaskService>();
            //services.AddTransient<IPensionRegulatorService, PensionRegulatorService>();

            //services.AddScoped<IReferenceDataApiClient, ReferenceDataApiClient>();

            //services.AddTransient<IDateTimeService, DateTimeService>();

            //services.AddTransient<IHashingService>(_ => new HashingService.HashingService(configuration.AllowedHashstringCharacters, configuration.Hashstring));

            //services.AddTransient<IUserAccountRepository, UserAccountRepository>();

            services.AddScoped(typeof(ICookieService<>), typeof(HttpCookieService<>));
            services.AddScoped(typeof(ICookieStorageService<>), typeof(CookieStorageService<>));
            services.AddScoped<IUrlActionHelper, UrlActionHelper>();

            services.AddScoped<IEncodingService, EncodingService>();

            services.AddTransient<HmrcExecutionPolicy>();

            services.AddTransient<IGenericEventFactory, GenericEventFactory>();
            //services.AddTransient<IPayeSchemeEventFactory, PayeSchemeEventFactory>();

            return services;
        }
    }
}
