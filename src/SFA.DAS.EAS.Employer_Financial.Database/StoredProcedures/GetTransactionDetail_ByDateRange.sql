﻿CREATE PROCEDURE [employer_financial].[GetTransactionDetail_ByDateRange]
	@accountId bigint,
	@fromDate datetime,
	@toDate datetime

AS
select 
    tl.AccountId,
    tl.LevyDeclared as Amount,
    t.Amount as EnglishFraction,
    ldt.Amount as TopUp,
    tl.EmpRef,
	null as CourseName,
	null as CourseLevel,
	null as CourseStartDate,
	null as ProviderName,
	null as ApprenticeName,
	null as ApprenticeNINumber,	
    tl.TransactionDate,
    tl.Amount as LineAmount,
    tl.TransactionType,
	null as UkPrn,
	null as PeriodEnd,
	DateCreated,
	ld.PayrollYear,
	ld.PayrollMonth,
	tl.SfaCoInvestmentAmount,
	tl.EmployerCoInvestmentAmount
from [employer_financial].TransactionLine tl
inner join [employer_financial].LevyDeclarationTopup ldt on ldt.SubmissionId = tl.SubmissionId
inner join [employer_financial].LevyDeclaration ld on ld.submissionid = tl.submissionid
OUTER APPLY
(
	SELECT TOP 1 Amount
	FROM [employer_financial].[EnglishFraction] ef
	WHERE ef.EmpRef = tl.empRef 
		AND ef.[DateCalculated] <= tl.TransactionDate
	ORDER BY [DateCalculated] DESC
) t
where    tl.DateCreated >= @fromDate AND 
        tl.DateCreated <= @toDate AND 
        tl.AccountId = @accountId 
		

union all

select 
    tl.AccountId,
    null as Amount,
    null as EnglishFraction,
    null as TopUp,
    null as empref,
	meta.ApprenticeshipCourseName as CourseName,
	meta.ApprenticeshipCourseLevel as CourseLevel,
	meta.ApprenticeshipCourseStartDate as CourseStartDate,
	meta.ProviderName as ProviderName,
	meta.ApprenticeName, 
	meta.ApprenticeNINumber,	
    (tl.TransactionDate) as transactiondate,
    (p.Amount) as LineAmount,
    tl.TransactionType,
	p.Ukprn as UkPrn,
	p.PeriodEnd as PeriodEnd,
	DateCreated,
	null as PayrollYear,
	null as PayrollMonth,
	tl.SfaCoInvestmentAmount,
	tl.EmployerCoInvestmentAmount
from [employer_financial].TransactionLine tl
inner join [employer_financial].Payment p on p.PeriodEnd = tl.PeriodEnd and p.AccountId = tl.AccountId and p.Ukprn = tl.UkPrn
inner join [employer_financial].PaymentMetaData meta on p.PaymentMetaDataId = meta.Id
where   tl.DateCreated >= @fromDate AND 
        tl.DateCreated <= @toDate AND 
        tl.AccountId = @accountId AND
		p.Ukprn = @ukprn