using System.Runtime.Serialization;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.Api.ServiceRegistrations
{
    public static class ApplicationServiceRegistrations
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddTransient<IEncodingService,EncodingService>();

            return services;
        }
    }
}
