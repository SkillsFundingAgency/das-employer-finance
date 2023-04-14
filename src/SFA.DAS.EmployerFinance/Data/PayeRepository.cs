using Microsoft.EntityFrameworkCore.Storage;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Models.Account;
using SFA.DAS.EmployerFinance.Models.Paye;

namespace SFA.DAS.EmployerFinance.Data;

public class PayeRepository : IPayeRepository
{
    private readonly Lazy<EmployerFinanceDbContext> _db;
    public PayeRepository(Lazy<EmployerFinanceDbContext> db)
    {
        _db = db;
    }

    public async Task<Paye> GetPayeSchemeByRef(string payeRef)
    {
        var parameters = new DynamicParameters();

        parameters.Add("@Ref", payeRef, DbType.String);

        var result = await _db.Value.Database.GetDbConnection().QueryAsync<Paye>(
            sql: "[employer_financial].[GetPaye_ByRef]",
            param: parameters,
            transaction: _db.Value.Database.CurrentTransaction?.GetDbTransaction(),
            commandType: CommandType.StoredProcedure);

        return result.SingleOrDefault();
    }

    public Task UpdatePayeSchemeName(string payeRef, string refName)
    {
        var parameters = new DynamicParameters();

        parameters.Add("@Ref", payeRef, DbType.String);
        parameters.Add("@RefName", refName, DbType.String);

        return _db.Value.Database.GetDbConnection().ExecuteAsync(
            sql: "[employer_financial].[UpdatePayeName_ByRef]",
            param: parameters,
            transaction: _db.Value.Database.CurrentTransaction?.GetDbTransaction(),
            commandType: CommandType.StoredProcedure);
    }

    public async Task<PayeSchemeView> GetPayeForAccountByRef(long accountId, string reference)
    {
        var parameters = new DynamicParameters();

        parameters.Add("@AccountId", accountId, DbType.Int64);
        parameters.Add("@Ref", reference, DbType.String);

        var result = await _db.Value.Database.GetDbConnection().QueryAsync<PayeSchemeView>(sql: "[employer_financial].[GetPayeForAccount_ByRef]", param: parameters,
            transaction: _db.Value.Database.CurrentTransaction?.GetDbTransaction(),
            commandType: CommandType.StoredProcedure);

        return result.SingleOrDefault();
    }

    public async Task<PayeSchemes> GetGovernmentGatewayOnlySchemesByEmployerId(long employerId)
    {
        var parameters = new DynamicParameters();

        parameters.Add("@accountId", employerId, DbType.Int64);

        var result = await _db.Value.Database.GetDbConnection().QueryAsync<Paye>(sql: "[employer_financial].[GetPayeSchemesAddedByGovernmentGateway_ByAccountId]", param: parameters,
            transaction: _db.Value.Database.CurrentTransaction?.GetDbTransaction(),
            commandType: CommandType.StoredProcedure);

        return new PayeSchemes
        {
            SchemesList = result.ToList()
        };
    }

    public Task CreatePayeScheme(Paye paye)
    {
        var parameters = new DynamicParameters();

        parameters.Add("@accountId", paye.AccountId, DbType.Int64);
        parameters.Add("@empRef", paye.EmpRef, DbType.String);
        parameters.Add("@name", paye.Name, DbType.String);
        parameters.Add("@aorn", paye.Aorn, DbType.String);

        return _db.Value.Database.GetDbConnection().ExecuteAsync(sql: "[employer_financial].[CreateAccountPaye]", param: parameters,
            transaction: _db.Value.Database.CurrentTransaction?.GetDbTransaction(),
            commandType: CommandType.StoredProcedure);
    }

    public Task RemovePayeScheme(long accountId, string payeRef)
    {
        var parameters = new DynamicParameters();

        parameters.Add("@accountId", accountId, DbType.Int64);
        parameters.Add("@empRef", payeRef, DbType.String);

        return _db.Value.Database.GetDbConnection().ExecuteAsync(sql: "[employer_financial].[RemoveAccountPaye]", param: parameters, 
            transaction: _db.Value.Database.CurrentTransaction?.GetDbTransaction(),
            commandType: CommandType.StoredProcedure);
    }
}