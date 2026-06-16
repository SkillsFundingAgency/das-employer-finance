using SFA.DAS.EmployerFinance.Models.Transfers;

namespace SFA.DAS.EmployerFinance.Extensions;

public static class TransferStagingExtensions
{
    public static DataTable ToTransferStagingDataTable(
        this IEnumerable<TransferStaging> transfers)
    {
        var table = new DataTable();

        table.Columns.Add("TransferId", typeof(long));
        table.Columns.Add("SenderAccountId", typeof(long));
        table.Columns.Add("ReceiverAccountId", typeof(long));
        table.Columns.Add("ReceiverAccountName", typeof(string));
        table.Columns.Add("Amount", typeof(decimal));
        table.Columns.Add("TransferDate", typeof(DateTime));
        table.Columns.Add("PeriodEnd", typeof(string));
        table.Columns.Add("CollectionPeriodMonth", typeof(int));
        table.Columns.Add("CollectionPeriodYear", typeof(int));
        table.Columns.Add("Ukprn", typeof(long));
        table.Columns.Add("CourseName", typeof(string));
        table.Columns.Add("CreatedBy", typeof(string));
        table.Columns.Add("CorrelationId", typeof(string));

        foreach (var t in transfers)
        {
            table.Rows.Add(
                t.TransferId,
                t.SenderAccountId,
                t.ReceiverAccountId,
                t.ReceiverAccountName,
                t.Amount,
                t.TransferDate,
                t.PeriodEnd,
                t.CollectionPeriodMonth,
                t.CollectionPeriodYear,
                t.Ukprn,
                t.CourseName,
                t.CreatedBy,
                t.CorrelationId
            );
        }

        return table;
    }
}