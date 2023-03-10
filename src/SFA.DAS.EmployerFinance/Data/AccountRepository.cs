using Microsoft.EntityFrameworkCore.Storage;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Data.Contracts;

namespace SFA.DAS.EmployerFinance.Data;

public class AccountRepository : BaseRepository, IAccountRepository
{
    private readonly Lazy<EmployerFinanceDbContext> _db;

    public AccountRepository(EmployerFinanceConfiguration configuration, ILogger<AccountRepository> logger, Lazy<EmployerFinanceDbContext> db)
        : base(configuration.DatabaseConnectionString, logger)
    {
        _db = db;
    }

    public async Task<string> GetAccountName(long accountId)
    {
        var parameters = new DynamicParameters();

        parameters.Add("@accountId", accountId, DbType.Int64);

        var result = await _db.Value.Database.GetDbConnection().QueryAsync<string>(
            sql: "SELECT Name FROM [employer_financial].[Account] WHERE Id = @accountId",
            transaction: _db.Value.Database.CurrentTransaction?.GetDbTransaction(),
            param: parameters,
            commandType: CommandType.Text);

        return result.SingleOrDefault();
    }

    public async Task<Dictionary<long, string>> GetAccountNames(IEnumerable<long> accountIds)
    {
        var result = await _db.Value.Database.GetDbConnection().QueryAsync<AccountNameItem>(
            sql: "SELECT Id, Name FROM [employer_financial].[Account] WHERE Id IN @accountIds",
            transaction: _db.Value.Database.CurrentTransaction?.GetDbTransaction(),
            param: new { accountIds = accountIds });

        return result.ToDictionary(d => d.Id, d => d.Name);
    }

    public async Task CreateAccount(long accountId, string name)
    {
        var parameters = new DynamicParameters();

        parameters.Add("@id", accountId, DbType.Int64);
        parameters.Add("@name", name, DbType.String);

        await _db.Value.Database.GetDbConnection().ExecuteAsync(
            sql: "[employer_financial].[CreateAccount]",
            transaction: _db.Value.Database.CurrentTransaction?.GetDbTransaction(),
            param: parameters,
            commandType: CommandType.StoredProcedure);
    }

    public async Task RenameAccount(long accountId, string name)
    {
        var parameters = new DynamicParameters();

        parameters.Add("@id", accountId, DbType.Int64);
        parameters.Add("@name", name, DbType.String);

        await _db.Value.Database.GetDbConnection().ExecuteAsync(
            sql: "[employer_financial].[RenameAccount]",
            transaction: _db.Value.Database.CurrentTransaction?.GetDbTransaction(),
            param: parameters,
            commandType: CommandType.StoredProcedure);
    }

    private class AccountNameItem
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }
}