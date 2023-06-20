using SFA.DAS.EmployerFinance.Models.UserProfile;

namespace SFA.DAS.EmployerFinance.Data.Contracts;

public interface IUserAccountRepository
{
    Task<User> Get(Guid @ref);
}