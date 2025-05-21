using SFA.DAS.EmployerFinance.Models.Levy;
using SFA.DAS.EmployerFinance.Web.ViewModels;

namespace SFA.DAS.EmployerFinance.Web.Orchestrators;

/// <summary>
/// Used for mocking as part of unit testing
/// </summary>
public interface IEmployerAccountTransactionsOrchestrator
{
    Task<OrchestratorResponse<FinanceDashboardViewModel>> Index(string hashedAccountId, ClaimsIdentity firstOrDefault);

    Task<OrchestratorResponse<PaymentTransactionViewModel>> FindAccountPaymentTransactions(
        string hashedId, long ukprn, DateTime fromDate, DateTime toDate);

    Task<OrchestratorResponse<ProviderPaymentsSummaryViewModel>> GetProviderPaymentSummary(
        string hashedAccountId, long ukprn, DateTime fromDate, DateTime toDate);

    Task<OrchestratorResponse<CoursePaymentDetailsViewModel>> GetCoursePaymentSummary(
        string hashedAccountId, long ukprn, string courseName, int? courseLevel, int? pathwayCode,
        DateTime fromDate, DateTime toDate);

    Task<OrchestratorResponse<TransactionViewResultViewModel>> GetAccountTransactions(
        string hashedId, int year, int month);

    Task<OrchestratorResponse<TransactionLineViewModel<LevyDeclarationTransactionLine>>> FindAccountLevyDeclarationTransactions(string hashedId, DateTime fromDate, DateTime toDate);
}