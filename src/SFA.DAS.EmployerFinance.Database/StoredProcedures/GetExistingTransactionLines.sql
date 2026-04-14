CREATE PROCEDURE [employer_financial].[GetExistingTransactionLines]
    @AccountId BIGINT,
    @PeriodEnd NVARCHAR(MAX),
    @TransactionType INT = 3
AS
 
    SELECT
          tl.[AccountId],
          tl.TransactionType,
          MAX(tl.TransactionDate) as TransactionDate,
          SUM(tl.Amount) as Amount,
          tl.Ukprn,
          tl.DateCreated,
          tl.SfaCoInvestmentAmount,
          tl.EmployerCoInvestmentAmount,
          tl.PeriodEnd,
          ld.PayrollYear,
          ld.PayrollMonth,
          tl.TransferSenderAccountId as SenderAccountId,
          tl.TransferSenderAccountName as SenderAccountName,
          tl.TransferReceiverAccountId as ReceiverAccountId,
          tl.TransferReceiverAccountName as ReceiverAccountName
    FROM [employer_financial].[TransactionLine] tl
    LEFT JOIN [employer_financial].[LevyDeclaration] ld
        ON ld.SubmissionId = tl.SubmissionId
    WHERE tl.AccountId = @AccountId
      AND tl.PeriodEnd = @PeriodEnd
      AND tl.TransactionType = @TransactionType
    GROUP BY
        tl.DateCreated,
        tl.AccountId,
        tl.Ukprn,
        tl.SfaCoInvestmentAmount,
        tl.EmployerCoInvestmentAmount,
        tl.TransactionType,
        tl.PeriodEnd,
        ld.PayrollMonth,
        ld.PayrollYear,
        tl.TransferSenderAccountId,
        tl.TransferSenderAccountName,
        tl.TransferReceiverAccountId,
        tl.TransferReceiverAccountName
    ORDER BY
        DateCreated DESC,
        TransactionType DESC,
        Ukprn DESC