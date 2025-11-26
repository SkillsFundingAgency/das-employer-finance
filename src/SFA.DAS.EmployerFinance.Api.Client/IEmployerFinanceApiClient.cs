using SFA.DAS.EmployerFinance.Api.Types;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerFinance.Api.Client
{
    public interface IEmployerFinanceApiClient
    {
        Task HealthCheck();

        Task<List<LevyDeclaration>> GetLevyDeclarations(string hashedAccountId);

        Task<List<LevyDeclaration>> GetLevyForPeriod(string hashedAccountId, string payrollYear, short payrollMonth);

        Task<Transactions> GetTransactions(string accountId, int year, int month);

        Task<List<TransactionSummary>> GetTransactionSummary(string accountId);

        Task<TotalPaymentsModel> GetFinanceStatistics();

        Task<List<AccountBalance>> GetAccountBalances(List<string> accountIds);

        Task<TransferAllowance> GetTransferAllowance(string hashedAccountId);

        Task<List<Account>> GetAllEmployerAccounts(int pageNumber, int pageSize = 10);

        Task<Account> GetAccount(long accountId);

        Task<List<Guid>> GetAccountPaymentIds(long accountId);
        
        Task<List<PeriodEnd>> GetAllPeriodEnds();

        Task<string> CreatePeriodEnd(PeriodEnd periodEnd);

        Task<PeriodEnd> GetPeriodEndByPeriodEndId(string periodEndId);
    }
}