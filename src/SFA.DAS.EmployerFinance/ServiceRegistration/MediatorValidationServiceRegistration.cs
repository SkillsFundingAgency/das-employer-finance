using SFA.DAS.EmployerFinance.Commands.CreateEnglishFractionCalculationDate;
using SFA.DAS.EmployerFinance.Commands.CreateNewPeriodEnd;
using SFA.DAS.EmployerFinance.Commands.CreateTransferTransactions;
using SFA.DAS.EmployerFinance.Commands.RefreshAccountTransfers;
using SFA.DAS.EmployerFinance.Commands.RefreshEmployerLevyData;
using SFA.DAS.EmployerFinance.Commands.RefreshPaymentData;
using SFA.DAS.EmployerFinance.Commands.RefreshPaymentMetadata;
using SFA.DAS.EmployerFinance.Commands.UpdatePayeInformation;
using SFA.DAS.EmployerFinance.Commands.UpsertRegisteredUser;
using SFA.DAS.EmployerFinance.Queries.FindAccountCoursePayments;
using SFA.DAS.EmployerFinance.Queries.FindAccountProviderPayments;
using SFA.DAS.EmployerFinance.Queries.FindEmployerAccountLevyDeclarationTransactions;
using SFA.DAS.EmployerFinance.Queries.GetAccountBalances;
using SFA.DAS.EmployerFinance.Queries.GetAccountFinanceOverview;
using SFA.DAS.EmployerFinance.Queries.GetContent;
using SFA.DAS.EmployerFinance.Queries.GetEmployerAccountDetail;
using SFA.DAS.EmployerFinance.Queries.GetEmployerAccountTransactions;
using SFA.DAS.EmployerFinance.Queries.GetEnglishFractionCurrent;
using SFA.DAS.EmployerFinance.Queries.GetEnglishFractionHistory;
using SFA.DAS.EmployerFinance.Queries.GetHMRCLevyDeclaration;
using SFA.DAS.EmployerFinance.Queries.GetLastLevyDeclaration;
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
        services.AddTransient<IValidator<GetHMRCLevyDeclarationQuery>, GetHMRCLevyDeclarationQueryValidator>();
        services.AddTransient<IValidator<GetLastLevyDeclarationQuery>, GetLastLevyDeclarationValidator>();

        services.AddTransient<IValidator<CreateEnglishFractionCalculationDateCommand>, CreateEnglishFractionCalculationDateCommandValidator>();
        services.AddTransient<IValidator<CreateNewPeriodEndCommand>, CreateNewPeriodEndCommandValidator>();
        services.AddTransient<IValidator<CreateTransferTransactionsCommand>, CreateTransferTransactionsCommandValidator>();
        services.AddTransient<IValidator<RefreshAccountTransfersCommand>, RefreshAccountTransfersCommandValidator>();
        services.AddTransient<IValidator<RefreshEmployerLevyDataCommand>, RefreshEmployerLevyDataCommandValidator>();
        services.AddTransient<IValidator<RefreshPaymentDataCommand>, RefreshPaymentDataCommandValidator>();
        services.AddTransient<IValidator<UpdatePayeInformationCommand>, UpdatePayeInformationValidator>();
        services.AddTransient<IValidator<RefreshPaymentMetadataCommand>, RefreshPaymentMetadataCommandValidator>();
    }

    public static void AddWebMediatorValidators(this IServiceCollection services)
    {
        services.AddTransient<IValidator<FindAccountCoursePaymentsQuery>, FindAccountCoursePaymentsQueryValidator>();
        services.AddTransient<IValidator<FindAccountProviderPaymentsQuery>, FindAccountProviderPaymentsQueryValidator>();
        services.AddTransient<IValidator<FindEmployerAccountLevyDeclarationTransactionsQuery>, FindEmployerAccountLevyDeclarationTransactionsQueryValidator>();
        services.AddTransient<IValidator<GetEmployerAccountTransactionsQuery>, GetEmployerAccountTransactionsValidator>();
        services.AddTransient<IValidator<GetPayeSchemeByRefQuery> , GetPayeSchemeByRefValidator>();
        services.AddTransient<IValidator<GetAccountFinanceOverviewQuery> , GetAccountFinanceOverviewQueryValidator>();
        services.AddTransient<IValidator<GetEmployerAccountDetailByHashedIdQuery> , GetEmployerAccountDetailByHashedIdValidator>();
        services.AddTransient<IValidator<UpsertRegisteredUserCommand> , UpsertRegisteredUserCommandValidator>();
        services.AddTransient<IValidator<GetContentQuery>, GetContentRequestValidator>();
    }
}