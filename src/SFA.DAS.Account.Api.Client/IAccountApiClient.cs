using System.Threading.Tasks;
using SFA.DAS.EAS.Account.Api.Types;

namespace SFA.DAS.EAS.Account.Api.Client
{
    public interface IAccountApiClient
    {
        Task<AccountDetailViewModel> GetAccount(string hashedAccountId);
        Task<AccountDetailViewModel> GetAccount(long accountId);
        Task<TransactionsViewModel> GetTransactions(string accountId, int year, int month);
        Task Ping();
    }
}