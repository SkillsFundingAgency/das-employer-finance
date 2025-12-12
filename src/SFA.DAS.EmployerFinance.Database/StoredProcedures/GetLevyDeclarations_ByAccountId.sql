CREATE PROCEDURE [employer_financial].[GetLevyDeclarations_ByAccountId] @AccountId bigint = 0 AS

SELECT x.*
FROM [employer_financial].[GetLevyDeclarationAndTopUp] x
WHERE x.AccountId = @AccountId
  AND (x.LastSubmission = 1 OR x.EndOfYearAdjustment = 1)
ORDER BY x.SubmissionDate ASC