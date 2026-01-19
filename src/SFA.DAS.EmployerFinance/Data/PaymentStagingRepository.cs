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

    public async Task<BulkPaymentsIngestResult> BulkInsertPaymentsAsync(List<PaymentStagingModel> payments)
    {
        var connection = _db.Value.Database.GetDbConnection() as SqlConnection;

        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync();

        var incomingPaymentIds = payments.Select(p => p.PaymentId).ToList();

        //checking the repeated submissions with same paymentIds
        var existingPaymentIds = new List<Guid>();
        var parameters = incomingPaymentIds.Select((id, index) => new SqlParameter($"@p{index}", id)).ToArray();
        var parameterNames = string.Join(",", parameters.Select(p => p.ParameterName));
        var sql = $"SELECT PaymentId FROM {TableName} WHERE PaymentId IN ({parameterNames})";

        using (var command = new SqlCommand(sql, connection))
        {
            command.Parameters.AddRange(parameters);
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    existingPaymentIds.Add(reader.GetGuid(0));
                }
            }
        }
        if (existingPaymentIds.Any())
        {
            return new BulkPaymentsIngestResult
            {
                IsSuccess = false,
                InsertedCount = 0,
                Message = $"Idempotency violation: The following PaymentIds already exist in the system: {string.Join(", ", existingPaymentIds)}"
            };
        }
        using var transaction = connection.BeginTransaction();
        try
        {
            DataTable paymentsTable = payments.ToPaymentStagingDataTable();

            using var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction);
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
            return new BulkPaymentsIngestResult { IsSuccess = true, InsertedCount = payments.Count, PaymentIds = incomingPaymentIds.ToList() };
        }
        catch (Exception)
        {
            transaction.Rollback();
            throw;
        }
    }
}