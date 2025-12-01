CREATE PROCEDURE [employer_financial].[ProcessDeclarationsTransactions]
	@AccountId BIGINT,
	@EmpRef NVARCHAR(50),
	@currentDate DATETIME = NULL,
	@expiryPeriod INT = NULL
AS

--Add the topup from the declaration
INSERT INTO [employer_financial].LevyDeclarationTopup
	select mainUpdate.* from
	(

	select 
		x.AccountId,
		DATEFROMPARTS(DatePart(yyyy,GETDATE()),DatePart(MM,GETDATE()),DATEPART(dd,GETDATE())) as DateAdded,
		x.SubmissionId,
		x.SubmissionDate,
		x.TopUp as Amount
	FROM 
		[employer_financial].[GetLevyDeclarationAndTopUp] x
	where
		x.LevyDueYTD is not null AND x.LastSubmission = 1 AND x.AccountId = @AccountId AND x.EmpRef = @EmpRef
	AND [employer_financial].[IsInDateLevy](@currentDate, @expiryPeriod, PayrollYear, PayrollMonth) = 1
	union all
	select
		x.AccountId,
		DATEFROMPARTS(DatePart(yyyy,GETDATE()),DatePart(MM,GETDATE()),DATEPART(dd,GETDATE())) as DateAdded,
		x.SubmissionId,
		x.SubmissionDate,
		((x.EndOfYearAdjustmentAmount * isnull(x.EnglishFraction,0))* isnull(x.TopUpPercentage,0) * -1) as Amount
	FROM 
		[employer_financial].[GetLevyDeclarationAndTopUp] x
	where
		x.LevyDueYTD is not null and x.EndOfYearAdjustment = 1  AND x.AccountId = @AccountId AND x.EmpRef = @EmpRef
	AND [employer_financial].[IsInDateLevy](@currentDate, @expiryPeriod, PayrollYear, PayrollMonth) = 1
	) mainUpdate
	WHERE NOT EXISTS (
		SELECT 1 
		FROM [employer_financial].LevyDeclarationTopup ldt 
		WHERE ldt.SubmissionId = mainUpdate.SubmissionId
	)


-- Create Declarations

DECLARE @updatedAccountTransactions table(Amount decimal(18,4))

INSERT INTO [employer_financial].TransactionLine
OUTPUT INSERTED.Amount INTO @updatedAccountTransactions
select mainUpdate.* from
	(
	select 
			x.AccountId,
			DATEFROMPARTS(DatePart(yyyy,x.CreatedDate),DatePart(MM,x.CreatedDate),DATEPART(dd,x.CreatedDate)) as DateCreated,
			x.SubmissionId as SubmissionId,
			x.SubmissionDate as TransactionDate,
			1 as TransactionType,
			x.LevyDeclaredInMonth as LevyDeclared,
			x.TotalAmount as Amount,		
			x.EmpRef as EmpRef,
			null as PeriodEnd,
			null as Ukprn,
			0 as SfaCoInvestmentAmount,
			0 as EmployerCoInvestmentAmount,
			x.EnglishFraction,
			null as TransferSenderAccountId,
			null as TransferSenderAccountName,
			null as TransferReceiverAccountId,
			null as TransferReceiverAccountName	
		FROM 
			[employer_financial].[GetLevyDeclarationAndTopUp] x
		where
			x.LevyDueYTD is not null AND x.LastSubmission = 1  AND x.AccountId = @AccountId AND x.EmpRef = @EmpRef
		AND [employer_financial].[IsInDateLevy](@currentDate, @expiryPeriod, PayrollYear, PayrollMonth) = 1
	union all	
		select 
			x.AccountId,
			DATEFROMPARTS(DatePart(yyyy,x.CreatedDate),DatePart(MM,x.CreatedDate),DATEPART(dd,x.CreatedDate)) as DateCreated,
			x.SubmissionId as SubmissionId,
			x.SubmissionDate as TransactionDate,
			1 as TransactionType,
			x.EndOfYearAdjustmentAmount * -1 as LevyDeclared,
			((x.EndOfYearAdjustmentAmount * ISNULL(x.EnglishFraction,1)) - ldt.Amount) * -1 as Amount,
			x.EmpRef as EmpRef,
			null as PeriodEnd,
			null as Ukprn,
			0 as SfaCoInvestmentAmount,
			0 as EmployerCoInvestmentAmount,
			ISNULL(x.EnglishFraction,1) AS EnglishFraction,
			null as TransferSenderAccountId,
			null as TransferSenderAccountName,
			null as TransferReceiverAccountId,
			null as TransferReceiverAccountName			
		FROM 
			[employer_financial].[GetLevyDeclarationAndTopUp] x 
		inner join
			[employer_financial].[LevyDeclarationTopup] ldt on ldt.SubmissionId = x.SubmissionId
		where x.EndOfYearAdjustment = 1  AND x.AccountId = @AccountId AND x.EmpRef = @EmpRef
		AND [employer_financial].[IsInDateLevy](@currentDate, @expiryPeriod, PayrollYear, PayrollMonth) = 1
	) mainUpdate
	WHERE NOT EXISTS (
		SELECT 1 
		FROM [employer_financial].TransactionLine tl 
		WHERE tl.SubmissionId = mainUpdate.SubmissionId 
		AND tl.TransactionType = 1
	)

	SELECT ISNULL(SUM(Amount), 0)
	FROM @updatedAccountTransactions
