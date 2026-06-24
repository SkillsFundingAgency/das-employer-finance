using SFA.DAS.EmployerFinance.Models.ExpiredFunds;

namespace SFA.DAS.EmployerFinance.Data.Contracts;

public interface IExpiredFundsRepository
{
    Task CreateDraft(long accountId, IEnumerable<ExpiredFund> expiredFunds, DateTime now, byte transactionType = 5);
    Task Create(long accountId, IEnumerable<ExpiredFund> expiredFunds, DateTime now, byte transactionType = 5);
    Task<IEnumerable<ExpiredFund>> Get(long accountId);
    Task<IEnumerable<ExpiredFund>> GetDraft(long accountId);
}