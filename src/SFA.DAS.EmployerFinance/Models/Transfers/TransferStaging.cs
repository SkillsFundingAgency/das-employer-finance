namespace SFA.DAS.EmployerFinance.Models.Transfers;

public class TransferStaging
{
    public long TransferId { get; set; }
    public long SenderAccountId { get; set; }
    public long ReceiverAccountId { get; set; }
    public string ReceiverAccountName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime TransferDate { get; set; }
    public string PeriodEnd { get; set; } = string.Empty;
    public short CollectionPeriodMonth { get; set; }
    public short CollectionPeriodYear { get; set; }
    public long Ukprn { get; set; }
    public string CourseName { get; set; } = string.Empty;
}
