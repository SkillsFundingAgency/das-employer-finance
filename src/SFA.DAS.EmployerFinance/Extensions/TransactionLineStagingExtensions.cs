using SFA.DAS.EmployerFinance.Models.TransactionLineStaging;

namespace SFA.DAS.EmployerFinance.Extensions;
public static class TransactionLineStagingExtensions
{
    public static DataTable ToTransactionLineStagingDataTable(this List<TransactionLineStagingModel> lines)
    {
        var table = new DataTable();
        table.Columns.Add(nameof(TransactionLineStagingModel.AccountId), typeof(long));
        table.Columns.Add(nameof(TransactionLineStagingModel.DateCreated), typeof(DateTime));
        AddNullableLongColumn(table, nameof(TransactionLineStagingModel.SubmissionId));
        table.Columns.Add(nameof(TransactionLineStagingModel.TransactionDate), typeof(DateTime));
        table.Columns.Add(nameof(TransactionLineStagingModel.TransactionType), typeof(byte));
        AddNullableDecimalColumn(table, nameof(TransactionLineStagingModel.LevyDeclared));
        table.Columns.Add(nameof(TransactionLineStagingModel.Amount), typeof(decimal));
        AddNullableStringColumn(table, nameof(TransactionLineStagingModel.EmpRef));
        AddNullableStringColumn(table, nameof(TransactionLineStagingModel.PeriodEnd));
        AddNullableLongColumn(table, nameof(TransactionLineStagingModel.Ukprn));
        table.Columns.Add(nameof(TransactionLineStagingModel.SfaCoInvestmentAmount), typeof(decimal));
        table.Columns.Add(nameof(TransactionLineStagingModel.EmployerCoInvestmentAmount), typeof(decimal));
        AddNullableDecimalColumn(table, nameof(TransactionLineStagingModel.EnglishFraction));
        AddNullableLongColumn(table, nameof(TransactionLineStagingModel.TransferSenderAccountId));
        AddNullableStringColumn(table, nameof(TransactionLineStagingModel.TransferSenderAccountName));
        AddNullableLongColumn(table, nameof(TransactionLineStagingModel.TransferReceiverAccountId));
        AddNullableStringColumn(table, nameof(TransactionLineStagingModel.TransferReceiverAccountName));

        foreach (var line in lines)
        {
            table.Rows.Add(
                line.AccountId,
                line.DateCreated,
                line.SubmissionId ?? (object)DBNull.Value,
                line.TransactionDate,
                checked((byte)line.TransactionType),
                line.LevyDeclared ?? (object)DBNull.Value,
                line.Amount,
                line.EmpRef ?? (object)DBNull.Value,
                line.PeriodEnd ?? (object)DBNull.Value,
                line.Ukprn ?? (object)DBNull.Value,
                line.SfaCoInvestmentAmount,
                line.EmployerCoInvestmentAmount,
                line.EnglishFraction ?? (object)DBNull.Value,
                line.TransferSenderAccountId ?? (object)DBNull.Value,
                line.TransferSenderAccountName ?? (object)DBNull.Value,
                line.TransferReceiverAccountId ?? (object)DBNull.Value,
                line.TransferReceiverAccountName ?? (object)DBNull.Value);
        }

        return table;
    }

    private static void AddNullableLongColumn(DataTable table, string columnName)
    {
        var column = table.Columns.Add(columnName, typeof(long));
        column.AllowDBNull = true;
    }

    private static void AddNullableDecimalColumn(DataTable table, string columnName)
    {
        var column = table.Columns.Add(columnName, typeof(decimal));
        column.AllowDBNull = true;
    }

    private static void AddNullableStringColumn(DataTable table, string columnName)
    {
        var column = table.Columns.Add(columnName, typeof(string));
        column.AllowDBNull = true;
    }
}
