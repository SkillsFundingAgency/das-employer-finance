using SFA.DAS.EmployerFinance.Models.UserProfile;

namespace SFA.DAS.EmployerFinance.Data.Contracts;

public interface IUserRepository
{
    Task Upsert(User user);
}