CREATE VIEW [employer_financial].[GetLevyDeclarationAndTopUp]
AS

WITH LatestSubmissionCTE AS
(
    SELECT 
        xld.EmpRef,
        xld.PayrollYear,
        xld.PayrollMonth,
        MAX(xld.SubmissionId) AS MaxSubmissionId
    FROM [employer_financial].LevyDeclaration xld
    WHERE xld.EndOfYearAdjustment = 0
        AND xld.SubmissionDate < [employer_financial].[CalculateSubmissionCutoffDate](xld.PayrollMonth, xld.PayrollYear)
        AND xld.SubmissionDate IN (
            SELECT MAX(ld2.SubmissionDate)
            FROM [employer_financial].LevyDeclaration ld2
            WHERE ld2.EmpRef = xld.EmpRef
                AND ld2.PayrollYear = xld.PayrollYear
                AND ld2.PayrollMonth = xld.PayrollMonth
                AND ld2.EndOfYearAdjustment = 0
                AND ld2.SubmissionDate < [employer_financial].[CalculateSubmissionCutoffDate](ld2.PayrollMonth, ld2.PayrollYear)
            GROUP BY ld2.EmpRef, ld2.PayrollYear, ld2.PayrollMonth
        )
    GROUP BY xld.EmpRef, xld.PayrollYear, xld.PayrollMonth
)
SELECT 
    ld.Id,
    ld.AccountId,
    ld.EmpRef,
    ld.SubmissionDate,
    ld.SubmissionId,
    ld.LevyDueYTD,
    ld.EnglishFraction,
    ld.TopUpPercentage,
    ld.PayrollYear,
    ld.PayrollMonth,
    ld.LastSubmission,
    ld.CreatedDate,
    ld.EndOfYearAdjustment,
    ld.EndOfYearAdjustmentAmount,
    ld.LevyAllowanceForYear,
    ld.DateCeased,
    ld.InactiveFrom,
    ld.InactiveTo,
    ld.HmrcSubmissionId,
    ld.NoPaymentForPeriod,
    ld.LevyDueYTD - ISNULL(prevMonth.LevyDueYTD, 0) AS LevyDeclaredInMonth,
    (ld.LevyDueYTD - ISNULL(prevMonth.LevyDueYTD, 0)) * ld.EnglishFraction * ISNULL(ld.TopUpPercentage, 0) AS TopUp,
    ((ld.LevyDueYTD - ISNULL(prevMonth.LevyDueYTD, 0)) * ld.EnglishFraction) + 
    ((ld.LevyDueYTD - ISNULL(prevMonth.LevyDueYTD, 0)) * ld.EnglishFraction * ISNULL(ld.TopUpPercentage, 0)) AS TotalAmount
FROM [employer_financial].[GetLevyDeclaration] ld
OUTER APPLY
(
    SELECT TOP 1 yld.LevyDueYTD
    FROM [employer_financial].LevyDeclaration yld
    INNER JOIN LatestSubmissionCTE yls
        ON yls.EmpRef = yld.EmpRef
        AND yls.PayrollYear = yld.PayrollYear
        AND yls.PayrollMonth = yld.PayrollMonth
        AND yls.MaxSubmissionId = yld.SubmissionId
    WHERE yld.EmpRef = ld.EmpRef
        AND yld.PayrollYear = ld.PayrollYear
        AND yld.PayrollMonth < ld.PayrollMonth
        AND yld.LevyDueYTD IS NOT NULL
        AND yld.EndOfYearAdjustment = 0
    ORDER BY yld.PayrollMonth DESC
) prevMonth

GO