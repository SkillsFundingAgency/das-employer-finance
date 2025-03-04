CREATE PROCEDURE [employer_financial].[GetLevyDeclarationsForPreviousMonths_ByAccountId]
    @AccountId bigint = 0,
    @months int
    
AS
SET NOCOUNT ON;

SELECT
    *
FROM [employer_financial].[GetLevyDeclarationAndTopUp]
WHERE EmpRef IN
(
    SELECT
        EmpRef
    FROM [employer_financial].LevyDeclaration
    WHERE AccountId = @AccountId
)
AND (LastSubmission = 1 OR EndOfYearAdjustment = 1)
AND AccountId = @AccountId
AND SubmissionDate >= DATEADD(month, -@months, GETDATE())
ORDER BY SubmissionDate ASC