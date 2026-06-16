using SFA.DAS.EmployerFinance.Models.PaymentStaging;

namespace SFA.DAS.EmployerFinance.Extensions;

public static class PaymentStagingExtensions
{
    public static DataTable ToPaymentStagingDataTable(this List<PaymentStagingModel> payments)
    {
        var table = new DataTable();
        table.Columns.Add("PaymentId", typeof(Guid));
        table.Columns.Add("AccountId", typeof(long));
        table.Columns.Add("Ukprn", typeof(long));
        table.Columns.Add("Uln", typeof(long));
        table.Columns.Add("ApprenticeshipId", typeof(long));
        table.Columns.Add("CollectionPeriodId", typeof(string));
        table.Columns.Add("DeliveryPeriodMonth", typeof(int));
        table.Columns.Add("DeliveryPeriodYear", typeof(int));
        table.Columns.Add("CollectionPeriodMonth", typeof(int));
        table.Columns.Add("CollectionPeriodYear", typeof(int));
        table.Columns.Add("FundingSource", typeof(string));
        table.Columns.Add("TransactionType", typeof(string));
        table.Columns.Add("Amount", typeof(decimal));
        table.Columns.Add("EvidenceSubmittedOn", typeof(DateTime));
        table.Columns.Add("EmployerAccountVersion", typeof(string));
        table.Columns.Add("ApprenticeshipVersion", typeof(string));

        foreach (var payment in payments)
        {
            table.Rows.Add(
                payment.PaymentId, payment.AccountId, payment.Ukprn, payment.Uln, payment.ApprenticeshipId,
                payment.CollectionPeriodId, payment.DeliveryPeriodMonth, payment.DeliveryPeriodYear,
                payment.CollectionPeriodMonth, payment.CollectionPeriodYear, payment.FundingSource,
                payment.TransactionType, payment.Amount, payment.EvidenceSubmittedOn,
                payment.EmployerAccountVersion, payment.ApprenticeshipVersion
            );
        }
        return table;
    }
}