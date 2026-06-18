CREATE PROCEDURE [employer_financial].[GetExpiredFunds]
	@AccountId bigint
AS

SELECT	YEAR(TransactionDate) AS CalendarPeriodYear,
		MONTH(TransactionDate) AS CalendarPeriodMonth,
		Amount,
		TransactionType
FROM	[employer_financial].[TransactionLine]
WHERE	AccountId = @AccountId
		AND TransactionType IN (/*ExpiredFund*/ 5, /*ShortTermExpiredFund*/ 6)
