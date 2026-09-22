CREATE PROCEDURE [employer_financial].[GetTransactionLines_ByAccountId]
    @AccountId BIGINT,
    @fromDate DATETIME,
    @toDate DATETIME
AS

    WITH TransferDetails AS (
        SELECT
            at.SenderAccountId,
            at.SenderAccountName,
            at.ReceiverAccountId,
            at.PeriodEnd,
            MAX(p.PaymentMetaDataId) AS PaymentMetaDataId,
            p.Ukprn
        FROM [employer_financial].[AccountTransfers] at
        INNER JOIN [employer_financial].[Payment] p
            ON p.AccountId = at.ReceiverAccountId
            AND p.ApprenticeshipId = at.ApprenticeshipId
            AND p.PeriodEnd = at.PeriodEnd
        GROUP BY
            at.SenderAccountId,
            at.SenderAccountName,
            at.ReceiverAccountId,
            at.PeriodEnd,
            p.Ukprn
    )
    SELECT
        tl.[AccountId],
        tl.TransactionType,
        MAX(tl.TransactionDate) as TransactionDate,
        Sum(tl.Amount) as Amount,
        tl.Ukprn,
        tl.DateCreated,
        tl.SfaCoInvestmentAmount,
        tl.EmployerCoInvestmentAmount,
        tl.PeriodEnd,
        ld.PayrollYear,
        ld.PayrollMonth,
        tl.TransferSenderAccountId as SenderAccountId,
        MAX(td.SenderAccountName) as SenderAccountName,
        tl.TransferReceiverAccountId as ReceiverAccountId,
        tl.TransferReceiverAccountName as ReceiverAccountName,
        MAX(pmd.ProviderName) as ProviderName
    FROM [employer_financial].[TransactionLine] tl
        LEFT JOIN [employer_financial].[LevyDeclaration] ld
            ON ld.submissionid = tl.submissionid
        LEFT JOIN TransferDetails td
            ON td.SenderAccountId = tl.TransferSenderAccountId
            AND td.ReceiverAccountId = tl.TransferReceiverAccountId
            AND td.PeriodEnd = tl.PeriodEnd
            AND td.Ukprn = tl.Ukprn
        LEFT JOIN [employer_financial].[PaymentMetaData] pmd
            ON pmd.Id = td.PaymentMetaDataId
    WHERE tl.AccountId = @accountId
      AND tl.DateCreated >= @fromDate
      AND tl.DateCreated <= @toDate
    GROUP BY
        tl.DateCreated,
        tl.AccountId,
        tl.UKPRN,
        tl.SfaCoInvestmentAmount,
        tl.EmployerCoInvestmentAmount,
        tl.TransactionType,
        tl.PeriodEnd,
        ld.PayrollMonth,
        ld.PayrollYear,
        tl.TransferSenderAccountId,
        tl.TransferReceiverAccountId,
        tl.TransferReceiverAccountName
    ORDER BY
        tl.DateCreated DESC,
        tl.TransactionType DESC,
        tl.UKPRN DESC