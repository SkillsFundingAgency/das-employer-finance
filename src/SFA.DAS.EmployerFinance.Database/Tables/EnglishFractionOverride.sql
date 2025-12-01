CREATE TABLE [employer_financial].[EnglishFractionOverride]
(
	[Id] BIGINT NOT NULL PRIMARY KEY IDENTITY, 
    [AccountId] BIGINT NOT NULL, 
    [EmpRef] NVARCHAR(50) NOT NULL, 
    [Amount] DECIMAL(18, 5) NOT NULL, 
    [CreatedOn] DATETIME NOT NULL DEFAULT GETDATE(), 
	[DateFrom] DATETIME NOT NULL, 
    [ApprovedBy] NVARCHAR(255) NOT NULL
)
GO

CREATE NONCLUSTERED INDEX [IX_EnglishFractionOverride_AccountId_EmpRef_DateFrom] 
ON [employer_financial].[EnglishFractionOverride] ([AccountId], [EmpRef], [DateFrom] DESC) 
INCLUDE ([Amount])
WITH (ONLINE = ON)
GO
