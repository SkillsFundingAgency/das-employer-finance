CREATE PROCEDURE [employer_financial].[GetLevyDeclarations_ByAccountId] @AccountId bigint = 0 AS
SELECT x.*
FROM [employer_financial].[GetLevyDeclarationAndTopUp] x
WHERE x.EmpRef IN
    (SELECT EmpRef
     FROM [employer_financial].LevyDeclaration
     WHERE AccountId = @AccountId )
  AND (x.LastSubmission = 1
       OR x.EndOfYearAdjustment = 1)
  AND x.AccountId = @AccountId
ORDER BY SubmissionDate ASC