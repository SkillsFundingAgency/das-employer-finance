CREATE PROCEDURE [employer_financial].[CreateExpiredFunds]
	@accountId BIGINT,
	@expiredFunds [employer_financial].[ExpiredFundsTable] READONLY,
	@now DATETIME,
	@transactionType TINYINT = 5,
	@correlationId NVARCHAR(100) = NULL
AS
	INSERT [employer_financial].[TransactionLine] (AccountId, DateCreated, CorrelationId, TransactionDate, TransactionType, Amount)
	SELECT @accountId, @now, @correlationId, datefromparts(CalendarPeriodYear,CalendarPeriodMonth,1), @transactionType, Amount
	FROM @expiredFunds
