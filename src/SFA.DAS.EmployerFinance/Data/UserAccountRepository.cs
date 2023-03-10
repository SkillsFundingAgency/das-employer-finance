using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Models.UserProfile;

namespace SFA.DAS.EmployerFinance.Data;

public class UserAccountRepository : IUserAccountRepository
{
    private readonly Lazy<EmployerFinanceDbContext> _db;

    public UserAccountRepository(Lazy<EmployerFinanceDbContext> db)
    {
        _db = db;
    }

    public Task<User> Get(Guid @ref)
    {
        return _db.Value.Users.SingleOrDefaultAsync(u => u.Ref == @ref);
    }
}