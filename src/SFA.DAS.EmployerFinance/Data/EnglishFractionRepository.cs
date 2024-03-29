﻿using Microsoft.EntityFrameworkCore.Storage;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Models.Levy;

namespace SFA.DAS.EmployerFinance.Data;

public class EnglishFractionRepository : IEnglishFractionRepository
{
    private readonly Lazy<EmployerFinanceDbContext> _db;

    public EnglishFractionRepository(Lazy<EmployerFinanceDbContext> db)
    {
        _db = db;
    }

    public Task CreateEmployerFraction(DasEnglishFraction fractions, string employerReference)
    {
        var parameters = new DynamicParameters();

        parameters.Add("@EmpRef", employerReference, DbType.String);
        parameters.Add("@Amount", fractions.Amount, DbType.Decimal);
        parameters.Add("@dateCalculated", fractions.DateCalculated, DbType.DateTime);

        return _db.Value.Database.GetDbConnection().ExecuteAsync(
            sql: "INSERT INTO [employer_financial].[EnglishFraction] (EmpRef, DateCalculated, Amount) VALUES (@empRef, @dateCalculated, @amount);",
            param: parameters,
            transaction: _db.Value.Database.CurrentTransaction?.GetDbTransaction(),
            commandType: CommandType.Text);
    }

    public Task<IEnumerable<DasEnglishFraction>> GetAllEmployerFractions(string employerReference)
    {
        var parameters = new DynamicParameters();

        parameters.Add("@empRef", employerReference, DbType.String);

        return _db.Value.Database.GetDbConnection().QueryAsync<DasEnglishFraction>(
            sql: "SELECT * FROM [employer_financial].[EnglishFraction] WHERE EmpRef = @empRef ORDER BY DateCalculated desc;",
            param: parameters,
            transaction: _db.Value.Database.CurrentTransaction?.GetDbTransaction(),
            commandType: CommandType.Text);
    }

    public async Task<DateTime> GetLastUpdateDate()
    {
        var result = await _db.Value.Database.GetDbConnection().QueryAsync<DateTime>(
            sql: "SELECT Top(1) DateCalculated FROM [employer_financial].[EnglishFractionCalculationDate] ORDER BY DateCalculated DESC;",
            transaction: _db.Value.Database.CurrentTransaction?.GetDbTransaction(),
            commandType: CommandType.Text);

        return result.FirstOrDefault();
    }

    public Task SetLastUpdateDate(DateTime dateUpdated)
    {
        var parameters = new DynamicParameters();

        parameters.Add("@dateCalculated", dateUpdated, DbType.Date);

        return _db.Value.Database.GetDbConnection().ExecuteAsync(
            sql: "INSERT INTO [employer_financial].[EnglishFractionCalculationDate] (DateCalculated) VALUES (@dateCalculated);",
            transaction: _db.Value.Database.CurrentTransaction?.GetDbTransaction(),
            param: parameters,
            commandType: CommandType.Text);
    }
}