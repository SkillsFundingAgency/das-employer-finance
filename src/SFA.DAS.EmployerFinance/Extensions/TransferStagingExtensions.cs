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
        table.Columns.Add("SenderAccountName", typeof(string));
        table.Columns.Add("ReceiverAccountId", typeof(long));
        table.Columns.Add("ReceiverAccountName", typeof(string));
        table.Columns.Add("Amount", typeof(decimal));
        table.Columns.Add("TransferDate", typeof(DateTime));
        table.Columns.Add("PeriodEnd", typeof(string));
        table.Columns.Add("CollectionPeriodMonth", typeof(int));
        table.Columns.Add("CollectionPeriodYear", typeof(int));
        table.Columns.Add("Ukprn", typeof(long));
        table.Columns.Add("CourseName", typeof(string));
        table.Columns.Add("CourseLevel", typeof(int));
        table.Columns.Add("LearningType", typeof(string));
        table.Columns.Add("ApprenticeshipId", typeof(long));
        table.Columns.Add("Type", typeof(string));
        table.Columns.Add("RequiredPaymentId", typeof(Guid));
        table.Columns.Add("CreatedBy", typeof(string));
        table.Columns.Add("CorrelationId", typeof(string));

        foreach (var t in transfers)
        {
            table.Rows.Add(
                t.TransferId,
                t.SenderAccountId,
                t.SenderAccountName ?? string.Empty,
                t.ReceiverAccountId,
                t.ReceiverAccountName,
                t.Amount,
                t.TransferDate,
                t.PeriodEnd,
                t.CollectionPeriodMonth,
                t.CollectionPeriodYear,
                t.Ukprn,
                string.IsNullOrEmpty(t.CourseName) ? DBNull.Value : t.CourseName,
                t.CourseLevel.HasValue ? t.CourseLevel.Value : DBNull.Value,
                string.IsNullOrEmpty(t.LearningType) ? DBNull.Value : t.LearningType,
                t.ApprenticeshipId,
                t.Type ?? string.Empty,
                t.RequiredPaymentId,
                t.CreatedBy,
                string.IsNullOrEmpty(t.CorrelationId) ? DBNull.Value : t.CorrelationId
            );
        }

        return table;
    }
}
