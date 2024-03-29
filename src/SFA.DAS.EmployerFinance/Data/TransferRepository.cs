﻿using Microsoft.EntityFrameworkCore.Storage;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Models.Transfers;

namespace SFA.DAS.EmployerFinance.Data;

public class TransferRepository : ITransferRepository
{
    private readonly Lazy<EmployerFinanceDbContext> _db;

    public TransferRepository(Lazy<EmployerFinanceDbContext> db)
    {
        _db = db;
    }

    public Task CreateAccountTransfers(IEnumerable<AccountTransfer> transfers)
    {
        var accountTransfers = transfers as AccountTransfer[] ?? transfers.ToArray();
        var transferDataTable = CreateTransferDataTable(accountTransfers);
        var parameters = new DynamicParameters();

        parameters.Add("@transfers", transferDataTable.AsTableValuedParameter("[employer_financial].[AccountTransferTable]"));

        return _db.Value.Database.GetDbConnection().ExecuteAsync(
            sql: "[employer_financial].[CreateAccountTransfersV1]",
            param: parameters,
            transaction: _db.Value.Database.CurrentTransaction?.GetDbTransaction(),
            commandType: CommandType.StoredProcedure,
            commandTimeout: 300);
    }

    public Task<IEnumerable<AccountTransfer>> GetReceiverAccountTransfersByPeriodEnd(long receiverAccountId, string periodEnd)
    {
        var parameters = new DynamicParameters();

        parameters.Add("@receiverAccountId", receiverAccountId, DbType.Int64);
        parameters.Add("@periodEnd", periodEnd, DbType.String);

        return _db.Value.Database.GetDbConnection().QueryAsync<AccountTransfer>(
            sql: "[employer_financial].[GetAccountTransfersByPeriodEnd]",
            param: parameters,
            transaction: _db.Value.Database.CurrentTransaction?.GetDbTransaction(),
            commandType: CommandType.StoredProcedure);
    }

    public Task<AccountTransferDetails> GetTransferPaymentDetails(AccountTransfer transfer)
    {
        var parameters = new DynamicParameters();

        parameters.Add("@receiverAccountId", transfer.ReceiverAccountId, DbType.Int64);
        parameters.Add("@periodEnd", transfer.PeriodEnd, DbType.String);
        parameters.Add("@apprenticeshipId", transfer.ApprenticeshipId, DbType.Int64);

        return _db.Value.Database.GetDbConnection().QuerySingleOrDefaultAsync<AccountTransferDetails>(
            sql: "[employer_financial].[GetTransferPaymentDetails]",
            param: parameters,
            transaction: _db.Value.Database.CurrentTransaction?.GetDbTransaction(),
            commandType: CommandType.StoredProcedure);
    }

    public async Task<TransferAllowance> GetTransferAllowance(long accountId, decimal transferAllowancePercentage)
    {
        var parameters = new DynamicParameters();

        parameters.Add("@accountId", accountId, DbType.Int64);
        parameters.Add("@allowancePercentage", transferAllowancePercentage, DbType.Decimal);

        var transferAllowance = await _db.Value.Database.GetDbConnection().QueryAsync<TransferAllowance>(
            sql: "[employer_financial].[GetAccountTransferAllowance]",
            param: parameters,
            transaction: _db.Value.Database.CurrentTransaction?.GetDbTransaction(),
            commandType: CommandType.StoredProcedure);

        return transferAllowance.SingleOrDefault() ?? new TransferAllowance();
    }

    private static DataTable CreateTransferDataTable(IEnumerable<AccountTransfer> transfers)
    {
        var table = new DataTable();

        table.Columns.AddRange(new[]
        {
            new DataColumn("SenderAccountId", typeof(long)),
            new DataColumn("SenderAccountName", typeof(string)),
            new DataColumn("ReceiverAccountId", typeof(long)),
            new DataColumn("ReceiverAccountName", typeof(string)),
            new DataColumn("ApprenticeshipId", typeof(long)),
            new DataColumn("CourseName", typeof(string)),
            new DataColumn("CourseLevel", typeof(int)),
            new DataColumn("Amount", typeof(decimal)),
            new DataColumn("PeriodEnd", typeof(string)),
            new DataColumn("Type", typeof(string)),
            new DataColumn("RequiredPaymentId", typeof(Guid)),
        });

        foreach (var transfer in transfers)
        {
            table.Rows.Add(
                transfer.SenderAccountId,
                transfer.SenderAccountName,
                transfer.ReceiverAccountId,
                transfer.ReceiverAccountName,
                transfer.ApprenticeshipId,
                transfer.CourseName,
                transfer.CourseLevel,
                transfer.Amount,
                transfer.PeriodEnd,
                transfer.Type,
                transfer.RequiredPaymentId);
        }

        table.AcceptChanges();

        return table;
    }
}