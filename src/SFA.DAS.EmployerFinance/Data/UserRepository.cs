using Microsoft.EntityFrameworkCore.Storage;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Models.UserProfile;

namespace SFA.DAS.EmployerFinance.Data;

public class UserRepository : IUserRepository
{
    private readonly EmployerFinanceDbContext _db;

    public UserRepository(EmployerFinanceDbContext db)
    {
        _db = db;
    }

    public Task Upsert(User user)
    {
        var parameters = new DynamicParameters();

        parameters.Add("@email", user.Email, DbType.String);
        parameters.Add("@userRef", user.Ref, DbType.Guid);
        parameters.Add("@firstName", user.FirstName, DbType.String);
        parameters.Add("@lastName", user.LastName, DbType.String);
        parameters.Add("@correlationId", user.CorrelationId, DbType.String);

        return _db.Database.GetDbConnection().ExecuteAsync(
            sql: "[employer_financial].[UpsertUser] @userRef, @email, @firstName, @lastName, @correlationId",
            param: parameters,
            transaction: _db.Database.CurrentTransaction?.GetDbTransaction(),
            commandType: CommandType.Text);
    }
}