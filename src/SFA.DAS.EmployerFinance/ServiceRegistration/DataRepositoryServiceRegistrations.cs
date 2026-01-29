using SFA.DAS.EmployerFinance.Data;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Interfaces;

namespace SFA.DAS.EmployerFinance.ServiceRegistration;

public static class DataRepositoryServiceRegistrations
{
    public static IServiceCollection AddDataRepositories(this IServiceCollection services)
    {
        services.AddScoped<IAccountLegalEntityRepository, AccountLegalEntityRepository>();
        services.AddScoped<IDasLevyRepository, DasLevyRepository>();
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

        services.AddFinanceDashboardRepository();

        return services;
    }

    public static IServiceCollection AddFinanceDashboardRepository(this IServiceCollection services)
    {
        services.AddScoped<FinanceDashboardRepositoryEf>();
        // services.AddScoped<FinanceDashboardRepositoryLegacy>();
        services.AddScoped<IFinanceDashboardRepository>(sp =>
        {
            var inner = sp.GetRequiredService<FinanceDashboardRepositoryEf>();
            // var inner = sp.GetRequiredService<FinanceDashboardRepositoryLegacy>();
            return new FinanceDashboardRepositoryWithCache(
                inner,
                sp.GetRequiredService<ICacheService>(),
                sp.GetRequiredService<ILogger<FinanceDashboardRepositoryWithCache>>());
        });

        return services;
    }
}