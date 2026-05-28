CREATE PROCEDURE [employer_financial].[GetLevyDeclarations_ByEmpRefPayrollMonthPayrollYear]
	@EmpRef VARCHAR(50),
	@PayrollYear VARCHAR(10),
	@PayrollMonth INT

AS

	SELECT
		CAST([SubmissionId] AS NVARCHAR(32)) AS Id,
		[LevyDueYTD] AS LevyDueYtd,
		ISNULL([SubmissionDate], '1900-01-01') AS SubmissionDate,
		[PayrollYear] AS PayrollYear,
		CAST([PayrollMonth] AS SMALLINT) AS PayrollMonth,
		ISNULL([HmrcSubmissionId], [SubmissionId]) AS SubmissionId

	FROM [employer_financial].[LevyDeclaration]
	WHERE [EmpRef] = @EmpRef
	  AND [PayrollYear] = @PayrollYear
	  AND [PayrollMonth] = @PayrollMonth

	ORDER BY [SubmissionDate] DESC;
 