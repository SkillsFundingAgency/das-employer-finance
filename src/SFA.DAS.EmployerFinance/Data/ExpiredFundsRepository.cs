﻿using Microsoft.EntityFrameworkCore.Storage;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Extensions;
using SFA.DAS.EmployerFinance.Models.ExpiredFunds;

namespace SFA.DAS.EmployerFinance.Data;

public class ExpiredFundsRepository : IExpiredFundsRepository
{
    private readonly Lazy<EmployerFinanceDbContext> _db;

    public ExpiredFundsRepository(Lazy<EmployerFinanceDbContext> db)
    {
        _db = db;
    }

    public Task CreateDraft(long accountId, IEnumerable<ExpiredFund> expiredFunds, DateTime now)
    {
        var expiredFundsTable = expiredFunds.ToExpiredFundsDataTable();

        var parameters = new DynamicParameters();

        parameters.Add("@accountId", accountId);
        parameters.Add("@expiredFunds", expiredFundsTable.AsTableValuedParameter("[employer_financial].[ExpiredFundsTable]"));
        parameters.Add("@now", now);

        return _db.Value.Database.GetDbConnection().ExecuteAsync(
             sql: "[employer_financial].[CreateDraftExpiredFunds]",
             transaction: _db.Value.Database.CurrentTransaction?.GetDbTransaction(),
             param: parameters,
             commandType: CommandType.StoredProcedure);
    }

    public Task Create(long accountId, IEnumerable<ExpiredFund> expiredFunds, DateTime now)
    {
        var expiredFundsTable = expiredFunds.ToExpiredFundsDataTable();

        var parameters = new DynamicParameters();

        parameters.Add("@accountId", accountId);
        parameters.Add("@expiredFunds", expiredFundsTable.AsTableValuedParameter("[employer_financial].[ExpiredFundsTable]"));
        parameters.Add("@now", now);

        return _db.Value.Database.GetDbConnection().ExecuteAsync(
             sql: "[employer_financial].[CreateExpiredFunds]",
             transaction: _db.Value.Database.CurrentTransaction?.GetDbTransaction(),
             param: parameters,
             commandType: CommandType.StoredProcedure);

    }

    public Task<IEnumerable<ExpiredFund>> Get(long accountId)
    {
        var parameters = new DynamicParameters();

        parameters.Add("@AccountId", accountId);

        return _db.Value.Database.GetDbConnection().QueryAsync<ExpiredFund>(
            "[employer_financial].[GetExpiredFunds]",
            transaction: _db.Value.Database.CurrentTransaction?.GetDbTransaction(),
            param: parameters,
            commandType: CommandType.StoredProcedure
        );
    }

    public Task<IEnumerable<ExpiredFund>> GetDraft(long accountId)
    {
        var parameters = new DynamicParameters();

        parameters.Add("@AccountId", accountId);

        return _db.Value.Database.GetDbConnection().QueryAsync<ExpiredFund>(
            "[employer_financial].[GetDraftExpiredFunds]",
            transaction: _db.Value.Database.CurrentTransaction?.GetDbTransaction(),
            param: parameters,
            commandType: CommandType.StoredProcedure
        );
    }
}