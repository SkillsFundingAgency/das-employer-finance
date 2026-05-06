namespace SFA.DAS.EmployerFinance.Models.TransactionLineStaging;

public class TransactionLineStagingModel
{
    public long Id { get; set; }

    public long AccountId { get; set; }
    public DateTime DateCreated { get; set; }
    public long? SubmissionId { get; set; }
    public DateTime TransactionDate { get; set; }
    public int TransactionType { get; set; }
    public decimal? LevyDeclared { get; set; }
    public decimal Amount { get; set; }
    public string? EmpRef { get; set; }
    public string PeriodEnd { get; set; }
    public long? Ukprn { get; set; }
    public decimal SfaCoInvestmentAmount { get; set; }
    public decimal EmployerCoInvestmentAmount { get; set; }
    public decimal? EnglishFraction { get; set; }
    public long? TransferSenderAccountId { get; set; }
    public string? TransferSenderAccountName { get; set; }
    public long? TransferReceiverAccountId { get; set; }
    public string? TransferReceiverAccountName { get; set; }
}