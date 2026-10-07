CREATE PROCEDURE [employer_financial].[GetTransferSenderAccountName]
    @AccountId BIGINT,
    @PeriodEnd VARCHAR(50)
AS

    SELECT
        p.Ukprn,
        MAX(at.SenderAccountName)                                      AS SenderAccountName,
        CAST(
            CASE WHEN COUNT(p.PaymentId) > COUNT(at.SenderAccountId)
                 THEN 1 ELSE 0 END
        AS BIT)                                                        AS IsPartialTransfer
    FROM [employer_financial].[Payment] p
        LEFT JOIN [employer_financial].[AccountTransfers] at
            ON at.ReceiverAccountId = p.AccountId
            AND at.ApprenticeshipId = p.ApprenticeshipId
            AND at.PeriodEnd = p.PeriodEnd
    WHERE p.AccountId = @AccountId
        AND p.PeriodEnd = @PeriodEnd
    GROUP BY p.Ukprn
    HAVING COUNT(at.SenderAccountId) > 0
