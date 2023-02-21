using SFA.DAS.EmployerFinance.Queries.FindAccountCoursePayments;
using SFA.DAS.EmployerFinance.Queries.FindAccountProviderPayments;
using SFA.DAS.EmployerFinance.Queries.FindEmployerAccountLevyDeclarationTransactions;
using SFA.DAS.EmployerFinance.Queries.GetAccountBalances;
using SFA.DAS.EmployerFinance.Queries.GetEmployerAccountDetail;
using SFA.DAS.EmployerFinance.Queries.GetEmployerAccountTransactions;
using SFA.DAS.EmployerFinance.Queries.GetEnglishFractionCurrent;
using SFA.DAS.EmployerFinance.Queries.GetEnglishFractionHistory;
using SFA.DAS.EmployerFinance.Queries.GetLevyDeclaration;
using SFA.DAS.EmployerFinance.Queries.GetPayeSchemeByRef;
using SFA.DAS.EmployerFinance.Validation;

namespace SFA.DAS.EmployerFinance.ServiceRegistration;

public static class MediatorValidationServiceRegistration
{
    public static void AddMediatorValidators(this IServiceCollection services)
    {
        services.AddTransient<IValidator<GetPayeSchemeByRefQuery>, GetPayeSchemeByRefValidator>();
        services.AddTransient<IValidator<GetEmployerAccountDetailByHashedIdQuery>, GetEmployerAccountDetailByHashedIdValidator>();
        services.AddTransient<IValidator<FindAccountCoursePaymentsQuery>, FindAccountCoursePaymentsQueryValidator>();
        services.AddTransient<IValidator<GetAccountBalancesRequest>, GetAccountBalancesValidator>();
        services.AddTransient<IValidator<GetEmployerAccountTransactionsQuery>, GetEmployerAccountTransactionsValidator>();
        services.AddTransient<IValidator<GetEnglishFractionCurrentQuery>, GetEnglishFractionCurrentQueryValidator>();
        services.AddTransient<IValidator<GetEnglishFractionHistoryQuery>, GetEnglishFractionHistoryQueryValidator>();
        services.AddTransient<IValidator<GetLevyDeclarationRequest>, GetLevyDeclarationValidator>();
        services.AddTransient<IValidator<FindAccountProviderPaymentsQuery>, FindAccountProviderPaymentsQueryValidator>();

        
    }

    public static void AddWebMediatorValidators(this IServiceCollection services)
    {
        services.AddTransient<IValidator<FindAccountCoursePaymentsQuery>, FindAccountCoursePaymentsQueryValidator>();
        services.AddTransient<IValidator<FindAccountProviderPaymentsQuery>, FindAccountProviderPaymentsQueryValidator>();
        services.AddTransient<IValidator<FindEmployerAccountLevyDeclarationTransactionsQuery>, FindEmployerAccountLevyDeclarationTransactionsQueryValidator>();
        services.AddTransient<IValidator<GetEmployerAccountTransactionsQuery>, GetEmployerAccountTransactionsValidator>();
        services.AddTransient<IValidator<GetPayeSchemeByRefQuery> , GetPayeSchemeByRefValidator>();
    }
}