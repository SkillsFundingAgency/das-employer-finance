using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.EmployerFinance.Data;
using SFA.DAS.EmployerFinance.Data.Contracts;

namespace SFA.DAS.EmployerFinance.ServiceRegistration
{
    public static class DataRepositoryServiceRegistrations
    {
        public static IServiceCollection AddDataRepositories(this IServiceCollection services)
        {
            services.AddScoped<IAccountLegalEntityRepository, AccountLegalEntityRepository>();
            services.AddScoped<IAccountRepository, AccountRepository>();
            services.AddScoped<IEmployerAccountRepository, EmployerAccountRepository>();
            services.AddScoped<IEnglishFractionRepository, EnglishFractionRepository>();
            services.AddScoped<IExpiredFundsRepository, ExpiredFundsRepository>();
            services.AddScoped<ILevyFundsInRepository, LevyFundsInRepository>();
            services.AddScoped<IPaymentFundsOutRepository, PaymentFundsOutRepository>();
            services.AddScoped<IPayeRepository, PayeRepository>();
            services.AddScoped<ITransactionRepository, TransactionRepository>();
            services.AddScoped<ITransferRepository, TransferRepository>();
            services.AddScoped<ITransferConnectionInvitationRepository, TransferConnectionInvitationRepository>();
            services.AddScoped<IUserAccountRepository, UserAccountRepository>();
            services.AddScoped<IUserRepository, UserRepository>();

            return services;
        }
    }
}
