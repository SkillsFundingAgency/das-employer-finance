using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.MarkerInterfaces;
using SFA.DAS.EmployerFinance.Models.Account;
using SFA.DAS.HashingService;
using SFA.DAS.NLog.Logger;
using SFA.DAS.Sql.Client;

namespace SFA.DAS.EmployerFinance.Data;

public class EmployerAccountRepository : IEmployerAccountRepository
{
    private readonly Lazy<EmployerFinanceDbContext> _db;

    public EmployerAccountRepository(Lazy<EmployerFinanceDbContext> db)
    {
        _db = db;
    }

    public async Task<Account> Get(long id)
    {
        var account = await _db.Value.Accounts.SingleOrDefaultAsync(a => a.Id == id);
        return account;
    }

    public async Task<List<Account>> Get(List<long> accountIds)
    {
        var accounts = await _db.Value.Accounts.Where(a => accountIds.Contains(a.Id)).ToListAsync();
        return accounts;
    }

    public async Task<Account> Get(string publicHashedId)
    {
        var parameters = new DynamicParameters();

        parameters.Add("@HashedAccountId", publicHashedId, DbType.String);

        var result = await _db.Value.Database.GetDbConnection().QueryAsync<Account>(
            sql: "select a.* from [employer_account].[Account] a where a.HashedId = @HashedAccountId;",
            param: parameters,
            commandType: CommandType.Text);

        return result.SingleOrDefault();
    }

    public async Task<List<Account>> GetAll()
    {
        var accounts = await _db.Value.Accounts.ToListAsync();
        return accounts;
    }
}