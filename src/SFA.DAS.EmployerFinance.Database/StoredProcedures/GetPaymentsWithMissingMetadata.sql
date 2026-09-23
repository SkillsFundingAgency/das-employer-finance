CREATE PROCEDURE [employer_financial].[GetPaymentsWithMissingMetadata]
    @PageNumber INT,
    @PageSize INT
AS

SELECT
    p.PaymentId AS Id
    ,p.AccountId AS EmployerAccountId
    ,p.PeriodEnd
FROM [employer_financial].[PaymentMetaData] pmd
JOIN [employer_financial].[Payment] p ON pmd.Id = p.PaymentMetaDataId
WHERE
    (pmd.ProviderName IS NULL
        OR
     pmd.ApprenticeName IS NULL)
ORDER BY p.DateImported DESC
OFFSET (@PageNumber - 1) * @PageSize ROWS
FETCH NEXT @PageSize ROWS ONLY
