using Microsoft.Data.SqlClient;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Extensions;
using SFA.DAS.EmployerFinance.Models.PaymentStaging;

namespace SFA.DAS.EmployerFinance.Data;

public class PaymentStagingRepository : IPaymentStagingRepository
{
    private readonly Lazy<EmployerFinanceDbContext> _db;
    private const string TableName = "[employer_financial].[PaymentStaging]";

    public PaymentStagingRepository(Lazy<EmployerFinanceDbContext> db)
    {
        _db = db;
    }

    public async Task<BulkPaymentsIngestResult> BulkInsertPaymentsAsync(
     List<PaymentStagingModel> payments)
    {
        var connection = _db.Value.Database.GetDbConnection() as SqlConnection;

        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        var incomingPaymentIds = payments.Select(p => p.PaymentId).ToList();

        using var transaction = connection.BeginTransaction();

        try
        {
            DataTable paymentsTable = payments.ToPaymentStagingDataTable();

            using var bulkCopy = new SqlBulkCopy(
                connection,
                SqlBulkCopyOptions.Default,
                transaction);

            bulkCopy.DestinationTableName = TableName;
            bulkCopy.BatchSize = 1000;
            bulkCopy.BulkCopyTimeout = 120;

            bulkCopy.ColumnMappings.Add(nameof(PaymentStagingModel.PaymentId), "PaymentId");
            bulkCopy.ColumnMappings.Add(nameof(PaymentStagingModel.AccountId), "AccountId");
            bulkCopy.ColumnMappings.Add(nameof(PaymentStagingModel.Ukprn), "Ukprn");
            bulkCopy.ColumnMappings.Add(nameof(PaymentStagingModel.Uln), "Uln");
            bulkCopy.ColumnMappings.Add(nameof(PaymentStagingModel.ApprenticeshipId), "ApprenticeshipId");
            bulkCopy.ColumnMappings.Add(nameof(PaymentStagingModel.CollectionPeriodId), "CollectionPeriodId");
            bulkCopy.ColumnMappings.Add(nameof(PaymentStagingModel.DeliveryPeriodMonth), "DeliveryPeriodMonth");
            bulkCopy.ColumnMappings.Add(nameof(PaymentStagingModel.DeliveryPeriodYear), "DeliveryPeriodYear");
            bulkCopy.ColumnMappings.Add(nameof(PaymentStagingModel.CollectionPeriodMonth), "CollectionPeriodMonth");
            bulkCopy.ColumnMappings.Add(nameof(PaymentStagingModel.CollectionPeriodYear), "CollectionPeriodYear");
            bulkCopy.ColumnMappings.Add(nameof(PaymentStagingModel.FundingSource), "FundingSource");
            bulkCopy.ColumnMappings.Add(nameof(PaymentStagingModel.TransactionType), "TransactionType");
            bulkCopy.ColumnMappings.Add(nameof(PaymentStagingModel.Amount), "Amount");
            bulkCopy.ColumnMappings.Add(nameof(PaymentStagingModel.EvidenceSubmittedOn), "EvidenceSubmittedOn");
            bulkCopy.ColumnMappings.Add(nameof(PaymentStagingModel.EmployerAccountVersion), "EmployerAccountVersion");
            bulkCopy.ColumnMappings.Add(nameof(PaymentStagingModel.ApprenticeshipVersion), "ApprenticeshipVersion");

            await bulkCopy.WriteToServerAsync(paymentsTable);

            transaction.Commit();

            return new BulkPaymentsIngestResult
            {
                IsSuccess = true,
                InsertedCount = payments.Count,
                PaymentIds = incomingPaymentIds
            };
        }
        catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
        {
            transaction.Rollback();

            return new BulkPaymentsIngestResult
            {
                IsSuccess = false,
                InsertedCount = 0,
                Message = "Idempotency violation: One or more PaymentIds already exist in the system."
            };
        }
        catch (Exception)
        {
            transaction.Rollback();
            throw;
        }
    }

}