using Microsoft.Data.SqlClient;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Extensions;
using SFA.DAS.EmployerFinance.Models.TransactionLineStaging;

namespace SFA.DAS.EmployerFinance.Data;

public class TransactionLineStagingRepository : ITransactionLineStagingRepository
{
    private readonly Lazy<EmployerFinanceDbContext> _db;
    private const string TableName = "[employer_financial].[TransactionLineStaging]";

    public TransactionLineStagingRepository(Lazy<EmployerFinanceDbContext> db)
    {
        _db = db;
    }

    public async Task BulkInsertTransactionLinesAsync(List<TransactionLineStagingModel> transactionLines)
    {
        var connection = _db.Value.Database.GetDbConnection() as SqlConnection;

        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        using var transaction = connection.BeginTransaction();

        try
        {
            var dataTable = transactionLines.ToTransactionLineStagingDataTable();

            using var bulkCopy = new SqlBulkCopy(
                connection,
                SqlBulkCopyOptions.Default,
                transaction);

            bulkCopy.DestinationTableName = TableName;
            bulkCopy.BatchSize = 1000;
            bulkCopy.BulkCopyTimeout = 120;

            bulkCopy.ColumnMappings.Add(nameof(TransactionLineStagingModel.AccountId), "AccountId");
            bulkCopy.ColumnMappings.Add(nameof(TransactionLineStagingModel.DateCreated), "DateCreated");
            bulkCopy.ColumnMappings.Add(nameof(TransactionLineStagingModel.SubmissionId), "SubmissionId");
            bulkCopy.ColumnMappings.Add(nameof(TransactionLineStagingModel.TransactionDate), "TransactionDate");
            bulkCopy.ColumnMappings.Add(nameof(TransactionLineStagingModel.TransactionType), "TransactionType");
            bulkCopy.ColumnMappings.Add(nameof(TransactionLineStagingModel.LevyDeclared), "LevyDeclared");
            bulkCopy.ColumnMappings.Add(nameof(TransactionLineStagingModel.Amount), "Amount");
            bulkCopy.ColumnMappings.Add(nameof(TransactionLineStagingModel.EmpRef), "EmpRef");
            bulkCopy.ColumnMappings.Add(nameof(TransactionLineStagingModel.PeriodEnd), "PeriodEnd");
            bulkCopy.ColumnMappings.Add(nameof(TransactionLineStagingModel.Ukprn), "Ukprn");
            bulkCopy.ColumnMappings.Add(nameof(TransactionLineStagingModel.SfaCoInvestmentAmount), "SfaCoInvestmentAmount");
            bulkCopy.ColumnMappings.Add(nameof(TransactionLineStagingModel.EmployerCoInvestmentAmount), "EmployerCoInvestmentAmount");
            bulkCopy.ColumnMappings.Add(nameof(TransactionLineStagingModel.EnglishFraction), "EnglishFraction");
            bulkCopy.ColumnMappings.Add(nameof(TransactionLineStagingModel.TransferSenderAccountId), "TransferSenderAccountId");
            bulkCopy.ColumnMappings.Add(nameof(TransactionLineStagingModel.TransferSenderAccountName), "TransferSenderAccountName");
            bulkCopy.ColumnMappings.Add(nameof(TransactionLineStagingModel.TransferReceiverAccountId), "TransferReceiverAccountId");
            bulkCopy.ColumnMappings.Add(nameof(TransactionLineStagingModel.TransferReceiverAccountName), "TransferReceiverAccountName");

            await bulkCopy.WriteToServerAsync(dataTable);

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}