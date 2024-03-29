﻿using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Models.Account;

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

    public async Task<List<Account>> GetAll()
    {
        var accounts = await _db.Value.Accounts.ToListAsync();
        return accounts;
    }
}