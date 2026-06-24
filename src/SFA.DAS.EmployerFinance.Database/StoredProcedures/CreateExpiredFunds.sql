CREATE PROCEDURE [employer_financial].[CreateExpiredFunds]
	@accountId BIGINT,
	@expiredFunds [employer_financial].[ExpiredFundsTable] READONLY,
	@now DATETIME,
	@transactionType TINYINT = 5
AS
	INSERT [employer_financial].[TransactionLine] (AccountId, DateCreated, TransactionDate, TransactionType, Amount)
	SELECT @accountId, @now, datefromparts(CalendarPeriodYear,CalendarPeriodMonth,1), @transactionType, Amount
	FROM @expiredFunds
