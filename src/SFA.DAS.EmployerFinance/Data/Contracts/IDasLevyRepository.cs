using SFA.DAS.EmployerFinance.Models.Account;
using SFA.DAS.EmployerFinance.Models.Levy;
using SFA.DAS.EmployerFinance.Models.Payments;
using SFA.DAS.EmployerFinance.Queries.GetAccountPaymentIds;
using SFA.DAS.EmployerFinance.Queries.GetAccounts;

namespace SFA.DAS.EmployerFinance.Data.Contracts;

public interface IDasLevyRepository
{
    Task CreateEmployerDeclarations(IEnumerable<DasDeclaration> declarations, string empRef, long accountId);
    Task CreateNewPeriodEnd(PeriodEnd periodEnd);
    Task CreatePayments(IEnumerable<PaymentDetails> payments);
    Task<ISet<Guid>> GetAccountPaymentIds(long accountId);
    Task<GetAccountPaymentIdsResponse> GetAccountPaymentIdsLinq(long accountId, int pageSize, int pageNumber);
    Task<IEnumerable<long>> GetEmployerDeclarationSubmissionIds(string empRef);
    Task<DasDeclaration> GetLastSubmissionForScheme(string empRef);
    Task<IEnumerable<PeriodEnd>> GetAllPeriodEnds();
    Task<PeriodEnd> GetPeriodEndById(string periodEndId);
    Task<DasDeclaration> GetSubmissionByEmprefPayrollYearAndMonth(string empRef, string payrollYear, short payrollMonth);
    Task<DasDeclaration> GetEffectivePeriod12Declaration(string empRef, string payrollYear, DateTime yearEndAdjustmentCutOff);
    Task<decimal> ProcessDeclarations(long accountId, string empRef);
    Task ProcessPaymentData(long accountId);
    Task<string> FindHistoricalProviderName(long ukprn);
    Task<List<LevyDeclarationItem>> GetAccountLevyDeclarations(long accountId);
    Task<List<LevyDeclarationItem>> GetAccountLevyDeclarationsForPreviousMonths(long accountId, int months);
    Task<List<LevyDeclarationItem>> GetAccountLevyDeclarations(long accountId, string payrollYear, short payrollMonth);
    Task<List<DasEnglishFraction>> GetEnglishFractionCurrent(long accountId, string[] empRefs);
    Task<List<AccountBalance>> GetAccountBalances(List<long> accountIds);
    Task<IEnumerable<DasEnglishFraction>> GetEnglishFractionHistory(long accountId, string empRef);
    Task UpdatePaymentMetadata(PaymentDetails details);
    Task<PaymentDetails> GetPaymentForPaymentDetails(Guid paymentId);
    Task<IEnumerable<PaymentDetails>> GetPaymentsWithMissingMetadata();
    Task<GetAccountsResponse> GetAccounts(int pageSize, int pageNumber);
    Task<Account> GetAccountById(long accountId);
}