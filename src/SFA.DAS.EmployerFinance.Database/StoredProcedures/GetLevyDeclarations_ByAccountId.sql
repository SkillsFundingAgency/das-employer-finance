CREATE PROCEDURE [employer_financial].[GetLevyDeclarations_ByAccountId] @AccountId bigint = 0 AS

SELECT 
    x.Id,
    x.AccountId,
    x.EmpRef,
    x.SubmissionDate,
    x.SubmissionId,
    x.LevyDueYTD,
    x.EnglishFraction,
    x.TopUpPercentage,
    x.PayrollYear,
    x.PayrollMonth,
    x.LastSubmission,
    x.CreatedDate,
    x.EndOfYearAdjustment,
    x.EndOfYearAdjustmentAmount,
    x.LevyAllowanceForYear,
    x.DateCeased,
    x.InactiveFrom,
    x.InactiveTo,
    x.HmrcSubmissionId,
    x.NoPaymentForPeriod,
    x.LevyDeclaredInMonth,
    x.TopUp,
    x.TotalAmount
FROM [employer_financial].[GetLevyDeclarationAndTopUp] x
WHERE x.AccountId = @AccountId
  AND (x.LastSubmission = 1 OR x.EndOfYearAdjustment = 1)
ORDER BY x.SubmissionDate ASC