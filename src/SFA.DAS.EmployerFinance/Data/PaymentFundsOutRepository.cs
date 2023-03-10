using Microsoft.EntityFrameworkCore.Storage;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Models.Payments;

namespace SFA.DAS.EmployerFinance.Data;

public class PaymentFundsOutRepository : BaseRepository, IPaymentFundsOutRepository
{
    private readonly Lazy<EmployerFinanceDbContext> _db;

    public PaymentFundsOutRepository(EmployerFinanceConfiguration configuration, ILogger<PaymentFundsOutRepository> logger, Lazy<EmployerFinanceDbContext> db)
        : base(configuration.DatabaseConnectionString, logger)
    {
        _db = db;
    }

    public Task<IEnumerable<PaymentFundsOut>> GetPaymentFundsOut(long accountId)
    {
        var parameters = new DynamicParameters();

        parameters.Add("@AccountId", accountId, DbType.Int64);

        return _db.Value.Database.GetDbConnection().QueryAsync<PaymentFundsOut>(
            "[employer_financial].[GetPaymentFundsOut]",
            param: parameters,
            transaction: _db.Value.Database.CurrentTransaction?.GetDbTransaction(),
            commandType: CommandType.StoredProcedure
        );
    }
}