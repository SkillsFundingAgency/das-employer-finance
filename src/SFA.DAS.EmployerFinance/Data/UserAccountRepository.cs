using System.Data.Entity;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Models.UserProfile;

namespace SFA.DAS.EmployerFinance.Data;

public class UserAccountRepository : BaseRepository, IUserAccountRepository
{
    private readonly Lazy<EmployerFinanceDbContext> _db;

    public UserAccountRepository(EmployerFinanceConfiguration configuration, ILogger<UserAccountRepository> logger, Lazy<EmployerFinanceDbContext> db)
        : base(configuration.DatabaseConnectionString, logger)
    {
        _db = db;
    }

    public Task<User> Get(Guid @ref)
    {
        return _db.Value.Users.SingleOrDefaultAsync(u => u.Ref == @ref);
    }
}