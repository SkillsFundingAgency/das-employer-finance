CREATE PROCEDURE [employer_financial].[GetTransferSenderAccountName]
    @AccountId BIGINT,
    @PeriodEnd VARCHAR(50)
AS

    SELECT DISTINCT
        p.Ukprn,
        at.SenderAccountName
    FROM [employer_financial].[AccountTransfers] at
    INNER JOIN [employer_financial].[Payment] p
        ON p.AccountId = at.ReceiverAccountId
        AND p.ApprenticeshipId = at.ApprenticeshipId
        AND p.PeriodEnd = at.PeriodEnd
    WHERE at.ReceiverAccountId = @AccountId
        AND at.PeriodEnd = @PeriodEnd
