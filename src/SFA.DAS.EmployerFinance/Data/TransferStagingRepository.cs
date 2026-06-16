using Microsoft.Data.SqlClient;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Extensions;
using SFA.DAS.EmployerFinance.Models.Transfers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerFinance.Data;

public class TransferStagingRepository : ITransferStagingRepository
{
    private readonly Lazy<EmployerFinanceDbContext> _db;
    private const string TableName = "[employer_financial].[TransferStaging]";

    public TransferStagingRepository(Lazy<EmployerFinanceDbContext> db)
    {
        _db = db;
    }

    public async Task BulkInsertTransfers(List<TransferStaging> transfers)
    {
        var connection = _db.Value.Database.GetDbConnection() as SqlConnection;

        if (connection!.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        using var transaction = connection.BeginTransaction();

        try
        {
            var dataTable = transfers.ToTransferStagingDataTable();

            using var bulkCopy = new SqlBulkCopy(
                connection,
                SqlBulkCopyOptions.Default,
                transaction);

            bulkCopy.DestinationTableName = TableName;
            bulkCopy.BatchSize = 1000;
            bulkCopy.BulkCopyTimeout = 120;

            bulkCopy.ColumnMappings.Add(nameof(TransferStaging.TransferId), "TransferId");
            bulkCopy.ColumnMappings.Add(nameof(TransferStaging.SenderAccountId), "SenderAccountId");
            bulkCopy.ColumnMappings.Add(nameof(TransferStaging.ReceiverAccountId), "ReceiverAccountId");
            bulkCopy.ColumnMappings.Add(nameof(TransferStaging.ReceiverAccountName), "ReceiverAccountName");
            bulkCopy.ColumnMappings.Add(nameof(TransferStaging.Amount), "Amount");
            bulkCopy.ColumnMappings.Add(nameof(TransferStaging.TransferDate), "TransferDate");
            bulkCopy.ColumnMappings.Add(nameof(TransferStaging.PeriodEnd), "PeriodEnd");
            bulkCopy.ColumnMappings.Add(nameof(TransferStaging.CollectionPeriodMonth), "CollectionPeriodMonth");
            bulkCopy.ColumnMappings.Add(nameof(TransferStaging.CollectionPeriodYear), "CollectionPeriodYear");
            bulkCopy.ColumnMappings.Add(nameof(TransferStaging.Ukprn), "Ukprn");
            bulkCopy.ColumnMappings.Add(nameof(TransferStaging.CourseName), "CourseName");
            bulkCopy.ColumnMappings.Add(nameof(TransferStaging.CreatedBy), "CreatedBy");
            bulkCopy.ColumnMappings.Add(nameof(TransferStaging.CorrelationId), "CorrelationId");

            await bulkCopy.WriteToServerAsync(dataTable);

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }


    public async Task<List<long>> GetExistingTransferIds(List<long> transferIds)
    {
        if (transferIds == null || transferIds.Count == 0)
        {
            return new List<long>();
        }

        return await _db.Value.TransferStaging
            .Where(x => transferIds.Contains(x.TransferId))
            .Select(x => x.TransferId)
            .ToListAsync();
    }

}