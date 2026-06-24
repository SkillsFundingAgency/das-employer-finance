IF (NOT EXISTS(SELECT TransactionType FROM [employer_financial].[TransactionLineTypes] WHERE TransactionType = 1))
BEGIN
	INSERT [employer_financial].[TransactionLineTypes] ([TransactionType], [Description]) VALUES (1, N'Levy')
END
IF (NOT EXISTS(SELECT TransactionType FROM [employer_financial].[TransactionLineTypes] WHERE TransactionType = 2))
BEGIN
	INSERT [employer_financial].[TransactionLineTypes] ([TransactionType], [Description]) VALUES (2, N'Levy Adjustment')
END
IF (NOT EXISTS(SELECT TransactionType FROM [employer_financial].[TransactionLineTypes] WHERE TransactionType = 3))
BEGIN
	INSERT [employer_financial].[TransactionLineTypes] ([TransactionType], [Description]) VALUES (3, N'Payment')
END
IF (EXISTS(SELECT TransactionType FROM [employer_financial].[TransactionLineTypes] WHERE TransactionType = 3 AND [Description] = 'Expired Levy'))
BEGIN
	UPDATE [employer_financial].[TransactionLineTypes] SET [Description] = N'Payment' WHERE TransactionType = 3
END
IF (NOT EXISTS(SELECT TransactionType FROM [employer_financial].[TransactionLineTypes] WHERE TransactionType = 4))
BEGIN
	INSERT [employer_financial].[TransactionLineTypes] ([TransactionType], [Description]) VALUES (4, N'Transfer')
END
IF (NOT EXISTS(SELECT TransactionType FROM [employer_financial].[TransactionLineTypes] WHERE TransactionType = 5))
BEGIN
	INSERT [employer_financial].[TransactionLineTypes] ([TransactionType], [Description]) VALUES (5, N'Expired Levy')
END
-- 6 is the new transaction type for 12 month expired levy, it has the same description as 5, but is a separate transaction type
-- so we can show it separately in the UI from the 24 month expired levy, transaction type 5
IF (NOT EXISTS(SELECT TransactionType FROM [employer_financial].[TransactionLineTypes] WHERE TransactionType = 6))
BEGIN
    INSERT [employer_financial].[TransactionLineTypes] ([TransactionType], [Description]) VALUES (6, N'Expired Levy')
END