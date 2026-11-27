CREATE PROCEDURE [employer_financial].[GetLatestTransactionTypeTransactionIdsForAccount]
	@AccountId bigint
AS

SELECT	TransactionType,
		MAX(Id) AS LatestId
FROM	[employer_financial].[TransactionLine]
WHERE	AccountId = @AccountId
GROUP BY TransactionType
