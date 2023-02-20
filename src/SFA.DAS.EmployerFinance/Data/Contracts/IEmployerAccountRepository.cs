using SFA.DAS.EmployerFinance.Models.Account;

namespace SFA.DAS.EmployerFinance.Data.Contracts;

public interface IEmployerAccountRepository
{
    Task<Account> Get(long id);
    Task<List<Account>> Get(List<long> accountIds);
    Task<Account> Get(string publicHashedId);
    Task<List<Account>> GetAll();
}