﻿using AutoMapper;
using Microsoft.EntityFrameworkCore.Storage;
using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Models.Levy;
using SFA.DAS.EmployerFinance.Models.Payments;
using SFA.DAS.EmployerFinance.Models.Transaction;
using SFA.DAS.EmployerFinance.Models.Transfers;
using SFA.DAS.EmployerFinance.Services;
using TransactionItemType = SFA.DAS.EmployerFinance.Models.Transaction.TransactionItemType;

namespace SFA.DAS.EmployerFinance.Data;

public class TransactionRepository : ITransactionRepository
{
    private readonly IMapper _mapper;
    private readonly Lazy<EmployerFinanceDbContext> _db;

    public TransactionRepository(IMapper mapper, Lazy<EmployerFinanceDbContext> db)
    {
        _mapper = mapper;
        _db = db;
    }

    public Task CreateTransferTransactions(IEnumerable<TransferTransactionLine> transactions)
    {
        var transactionTable = CreateTransferTransactionDataTable(transactions);
        var parameters = new DynamicParameters();

        parameters.Add("@transferTransactions",
            transactionTable.AsTableValuedParameter("[employer_financial].[TransferTransactionsTable]"));

        return _db.Value.Database.GetDbConnection().ExecuteAsync(
            sql: "[employer_financial].[CreateAccountTransferTransactions]",
            param: parameters,
            transaction: _db.Value.Database.CurrentTransaction?.GetDbTransaction(),
            commandType: CommandType.StoredProcedure);
    }

    public async Task<int> GetPreviousTransactionsCount(long accountId, DateTime fromDate)
    {
        var parameters = new DynamicParameters();

        parameters.Add("@accountId", accountId, DbType.Int64);
        parameters.Add("@fromDate", new DateTime(fromDate.Year, fromDate.Month, fromDate.Day), DbType.DateTime);

        return await _db.Value.Database.GetDbConnection().ExecuteScalarAsync<int>(
            sql: "[employer_financial].[GetPreviousTransactionsCount]",
            param: parameters,
            transaction: _db.Value.Database.CurrentTransaction?.GetDbTransaction(),
            commandType: CommandType.StoredProcedure);

    }

    public Task<decimal> GetAccountBalance(long accountId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@accountId", accountId, DbType.Int64);

        return _db.Value.Database.GetDbConnection().QuerySingleAsync<decimal>(
            sql: "SELECT ISNULL(SUM(Amount),0) FROM [employer_financial].[TransactionLine] WHERE AccountId = @accountId AND TransactionType in (1, 2, 3, 4, 5)",
            param: parameters,
            transaction: _db.Value.Database.CurrentTransaction?.GetDbTransaction(),
            commandType: CommandType.Text);
    }

    public async Task<TransactionLine[]> GetAccountTransactionsByDateRange(long accountId, DateTime fromDate, DateTime toDate)
    {
        var parameters = new DynamicParameters();

        parameters.Add("@accountId", accountId, DbType.Int64);
        parameters.Add("@fromDate", new DateTime(fromDate.Year, fromDate.Month, fromDate.Day), DbType.DateTime);
        parameters.Add("@toDate", new DateTime(toDate.Year, toDate.Month, toDate.Day, 23, 59, 59), DbType.DateTime);

        var result = await _db.Value.Database.GetDbConnection().QueryAsync<TransactionEntity>(
            sql: "[employer_financial].[GetTransactionLines_ByAccountId]",
            param: parameters,
            transaction: _db.Value.Database.CurrentTransaction?.GetDbTransaction(),
            commandType: CommandType.StoredProcedure);

        return MapTransactions(result);
    }

    private static DataTable CreateTransferTransactionDataTable(IEnumerable<TransferTransactionLine> transactions)
    {
        var table = new DataTable();

        table.Columns.AddRange(new[]
        {
            new DataColumn("AccountId", typeof(long)),
            new DataColumn("SenderAccountId", typeof(long)),
            new DataColumn("SenderAccountName", typeof(string)),
            new DataColumn("ReceiverAccountId", typeof(long)),
            new DataColumn("ReceiverAccountName", typeof(string)),
            new DataColumn("PeriodEnd", typeof(string)),
            new DataColumn("Amount", typeof(decimal)),
            new DataColumn("TransactionType", typeof(short)),
            new DataColumn("TransactionDate", typeof(DateTime)),
        });

        foreach (var transaction in transactions)
        {
            table.Rows.Add(
                transaction.AccountId,
                transaction.SenderAccountId,
                transaction.SenderAccountName,
                transaction.ReceiverAccountId,
                transaction.ReceiverAccountName,
                transaction.PeriodEnd,
                transaction.Amount,
                (short)TransactionItemType.Transfer,
                transaction.TransactionDate);
        }

        table.AcceptChanges();

        return table;
    }

    public async Task<TransactionLine[]> GetAccountTransactionByProviderAndDateRange(long accountId, long ukprn, DateTime fromDate, DateTime toDate)
    {
        var parameters = new DynamicParameters();

        parameters.Add("@accountId", accountId, DbType.Int64);
        parameters.Add("@ukprn", ukprn, DbType.Int64);
        parameters.Add("@fromDate", new DateTime(fromDate.Year, fromDate.Month, fromDate.Day), DbType.DateTime);
        parameters.Add("@toDate", new DateTime(toDate.Year, toDate.Month, toDate.Day, 23, 59, 59), DbType.DateTime);

        var result = await _db.Value.Database.GetDbConnection().QueryAsync<TransactionEntity>(
            sql: "[employer_financial].[GetPaymentDetail_ByAccountProviderAndDateRange]",
            param: parameters,
            transaction: _db.Value.Database.CurrentTransaction?.GetDbTransaction(),
            commandType: CommandType.StoredProcedure);

        return MapTransactions(result);
    }

    private TransactionLine[] MapTransactions(IEnumerable<TransactionEntity> transactionEntities)
    {
        return transactionEntities.Select(entity =>
        {
            switch (entity.TransactionType)
            {
                case TransactionItemType.Declaration:
                case TransactionItemType.TopUp:
                    return _mapper.Map<LevyDeclarationTransactionLine>(entity);

                case TransactionItemType.Payment:
                    return _mapper.Map<PaymentTransactionLine>(entity);

                case TransactionItemType.Transfer:
                    return _mapper.Map<TransferTransactionLine>(entity);

                case TransactionItemType.ExpiredFund:
                    return _mapper.Map<ExpiredFundTransactionLine>(entity);

                case TransactionItemType.Unknown:
                    return _mapper.Map<TransactionLine>(entity);

                default:
                    throw new InvalidOperationException($"Unable to map from {entity.TransactionType}.");
            }
        }).ToArray();
    }

    public async Task<TransactionLine[]> GetAccountCoursePaymentsByDateRange(long accountId, long ukprn, string courseName, int? courseLevel, int? pathwayCode, DateTime fromDate, DateTime toDate)
    {
        var parameters = new DynamicParameters();

        parameters.Add("@accountId", accountId, DbType.Int64);
        parameters.Add("@ukprn", ukprn, DbType.Int64);
        parameters.Add("@courseName", courseName, DbType.String);
        parameters.Add("@courseLevel", courseLevel, DbType.Int32);
        parameters.Add("@pathwayCode", pathwayCode, DbType.Int32);
        parameters.Add("@fromDate", new DateTime(fromDate.Year, fromDate.Month, fromDate.Day), DbType.DateTime);
        parameters.Add("@toDate", new DateTime(toDate.Year, toDate.Month, toDate.Day, 23, 59, 59), DbType.DateTime);

        var result = await _db.Value.Database.GetDbConnection().QueryAsync<TransactionEntity>(
            sql: "[employer_financial].[GetPaymentDetail_ByAccountProviderCourseAndDateRange]",
            param: parameters,
            transaction: _db.Value.Database.CurrentTransaction?.GetDbTransaction(),
            commandType: CommandType.StoredProcedure);

        return MapTransactions(result);
    }
    public async Task<TransactionLine[]> GetAccountLevyTransactionsByDateRange(long accountId, DateTime fromDate, DateTime toDate)
    {
        var parameters = new DynamicParameters();

        parameters.Add("@accountId", accountId, DbType.Int64);
        parameters.Add("@fromDate", new DateTime(fromDate.Year, fromDate.Month, fromDate.Day), DbType.DateTime);
        parameters.Add("@toDate", new DateTime(toDate.Year, toDate.Month, toDate.Day, 23, 59, 59), DbType.DateTime);

        var result = await _db.Value.Database.GetDbConnection().QueryAsync<TransactionEntity>(
            sql: "[employer_financial].[GetLevyDetail_ByAccountIdAndDateRange]",
            param: parameters,
            transaction: _db.Value.Database.CurrentTransaction?.GetDbTransaction(),
            commandType: CommandType.StoredProcedure);

        return MapTransactions(result);
    }
    
    public Task<string> GetProviderName(long ukprn, long accountId, string periodEnd)
    {
        var parameters = new DynamicParameters();

        parameters.Add("@ukprn", ukprn, DbType.Int64);
        parameters.Add("@accountId", accountId, DbType.Int64);
        parameters.Add("@periodEnd", periodEnd, DbType.String);

        return _db.Value.Database.GetDbConnection().ExecuteScalarAsync<string>(
            sql: "[employer_financial].[GetProviderName]",
            param: parameters,
            transaction: _db.Value.Database.CurrentTransaction?.GetDbTransaction(),
            commandType: CommandType.StoredProcedure);

    }

    public async Task<TransactionDownloadLine[]> GetAllTransactionDetailsForAccountByDate(long accountId, DateTime fromDate, DateTime toDate)
    {
        var parameters = new DynamicParameters();

        parameters.Add("@AccountId", accountId, DbType.Int64);
        parameters.Add("@fromDate", fromDate, DbType.DateTime);
        parameters.Add("@toDate", toDate, DbType.DateTime);

        var result = await _db.Value.Database
            .GetDbConnection()
            .QueryAsync<TransactionDownloadLine>(
                sql: "[employer_financial].[GetAllTransactionDetailsForAccountByDate]",
                param: parameters,
                transaction: _db.Value.Database.CurrentTransaction?.GetDbTransaction(),
                commandType: CommandType.StoredProcedure);

        var hmrcDateService = new HmrcDateService();
        var transactionDownloadLines = result as TransactionDownloadLine[] ?? result.ToArray();

        foreach (var res in transactionDownloadLines)
        {
            if (!string.IsNullOrEmpty(res.PayrollYear) && res.PayrollMonth != 0)
            {
                res.PeriodEnd = hmrcDateService.GetDateFromPayrollYearMonth(res.PayrollYear, res.PayrollMonth).ToString("MMM yyyy");
            }
        }

        return transactionDownloadLines.OrderByDescending(txn => txn.DateCreated).ToArray();
    }

    public Task<decimal> GetTotalSpendForLastYear(long accountId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@AccountId", accountId, DbType.Int64);

        return _db.Value.Database
            .GetDbConnection()
            .ExecuteScalarAsync<decimal>(sql: "[employer_financial].[GetTotalSpendForLastYearByAccountId]",
                param: parameters,
                transaction: _db.Value.Database.CurrentTransaction?.GetDbTransaction(),
                commandType: CommandType.StoredProcedure);
    }

    public async Task<List<TransactionSummary>> GetAccountTransactionSummary(long accountId)
    {
        var parameters = new DynamicParameters();

        parameters.Add("@AccountId", accountId, DbType.Int64);

        var result = await _db.Value.Database
            .GetDbConnection()
            .QueryAsync<TransactionSummary>(
                sql: "[employer_financial].[GetTransactionSummary_ByAccountId]",
                param: parameters,
                transaction: _db.Value.Database.CurrentTransaction?.GetDbTransaction(),
                commandType: CommandType.StoredProcedure);

        return result.ToList();
    }
}