using Microsoft.EntityFrameworkCore.Storage;
using SFA.DAS.EmployerFinance.Data.Contracts;

namespace SFA.DAS.EmployerFinance.Data;

public class AccountLegalEntityRepository : IAccountLegalEntityRepository
{
    private readonly Lazy<EmployerFinanceDbContext> _db;

    public AccountLegalEntityRepository(Lazy<EmployerFinanceDbContext> db)
    {
        _db = db;
    }

    public Task CreateAccountLegalEntity(long id, long? pendingAgreementId, long? signedAgreementId,
        int? signedAgreementVersion, long accountId, long legalEntityId)
    {
        var parameters = new DynamicParameters();

        parameters.Add("@id", id, DbType.Int64);
        parameters.Add("@accountId", accountId, DbType.Int64);
        parameters.Add("@legalEntityId", legalEntityId, DbType.Int64);
        parameters.Add("@signedAgreementVersion", signedAgreementVersion, DbType.Int32);
        parameters.Add("@signedAgreementId", signedAgreementId, DbType.Int64);
        parameters.Add("@pendingAgreementId", pendingAgreementId, DbType.Int64);

        return _db.Value.Database.GetDbConnection().ExecuteAsync(
            "[employer_financial].[CreateAccountLegalEntity]",
            parameters,
            transaction: _db.Value.Database.CurrentTransaction?.GetDbTransaction(),
            commandType: CommandType.StoredProcedure);
    }

    public Task SignAgreement(long signedAgreementId, int signedAgreementVersion, long accountId, long legalEntityId)
    {
        var parameters = new DynamicParameters();

        parameters.Add("@accountId", accountId, DbType.Int64);
        parameters.Add("@legalEntityId", legalEntityId, DbType.Int64);
        parameters.Add("@signedAgreementVersion", signedAgreementVersion, DbType.Int32);
        parameters.Add("@signedAgreementId", signedAgreementId, DbType.Int64);

        return _db.Value.Database.GetDbConnection().ExecuteAsync(
            "[employer_financial].[SignAccountLegalEntityAgreement]",
            parameters,
            transaction: _db.Value.Database.CurrentTransaction?.GetDbTransaction(),
            commandType: CommandType.StoredProcedure);
    }

    public Task RemoveAccountLegalEntity(long id)
    {
        var parameters = new DynamicParameters();

        parameters.Add("@id", id, DbType.Int64);

        return _db.Value.Database.GetDbConnection().ExecuteAsync(
            "[employer_financial].[RemoveAccountLegalEntity]",
            parameters,
            transaction: _db.Value.Database.CurrentTransaction?.GetDbTransaction(),
            commandType: CommandType.StoredProcedure);
    }
}