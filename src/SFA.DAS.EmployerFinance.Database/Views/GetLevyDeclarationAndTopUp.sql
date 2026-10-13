CREATE VIEW [employer_financial].[GetLevyDeclarationAndTopUp]
AS

SELECT *,
	   (decPlusLevy.LevyDeclaredInMonth * decPlusLevy.EnglishFraction) + decPlusLevy.TopUp as TotalAmount
FROM
	(SELECT *,
		   (dec.LevyDeclaredInMonth * dec.EnglishFraction) * dec.TopUpPercentage as TopUp
	FROM
		(SELECT 
			ld.*,
			-- End of year adjustments are a delta against the effective P12 declaration, not the previous month,
			-- so use the stored (inverted) adjustment amount as ProcessDeclarationsTransactions does
			CASE WHEN ld.EndOfYearAdjustment = 1
				THEN -ld.EndOfYearAdjustmentAmount
				ELSE ld.LevyDueYTD - ISNULL(y.LevyDueYTD, 0)
			END AS LevyDeclaredInMonth
		FROM [employer_financial].[GetLevyDeclaration] ld
		outer apply
		(
			SELECT TOP 1 LevyDueYTD
			FROM [employer_financial].[GetLevyDeclaration] y
			WHERE y.EmpRef = ld.EmpRef AND y.PayrollYear = ld.PayrollYear AND y.LastSubmission = 1 AND y.PayrollMonth < ld.PayrollMonth AND y.LevyDueYTD IS NOT NULL
			ORDER BY y.PayrollMonth DESC
		) y
	) dec
) decPlusLevy

GO