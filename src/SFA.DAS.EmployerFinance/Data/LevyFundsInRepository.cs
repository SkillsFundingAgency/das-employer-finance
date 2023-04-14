using Microsoft.EntityFrameworkCore.Storage;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Models.Levy;

namespace SFA.DAS.EmployerFinance.Data;

public class LevyFundsInRepository : ILevyFundsInRepository
{
    private readonly Lazy<EmployerFinanceDbContext> _db;

    public LevyFundsInRepository(Lazy<EmployerFinanceDbContext> db)
    {
        _db = db;
    }

    public Task<IEnumerable<LevyFundsIn>> GetLevyFundsIn(long accountId)
    {
        var parameters = new DynamicParameters();

        parameters.Add("@AccountId", accountId, DbType.Int64);

        return _db.Value.Database.GetDbConnection().QueryAsync<LevyFundsIn>(
            "[employer_financial].[GetLevyFundsIn]",
            param: parameters,
            transaction: _db.Value.Database.CurrentTransaction?.GetDbTransaction(),
            commandType: CommandType.StoredProcedure
        );
    }
}