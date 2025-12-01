CREATE TABLE [employer_financial].[TopUpPercentage]
(
	[Id] BIGINT NOT NULL PRIMARY KEY IDENTITY, 
    [DateFrom] DATETIME NOT NULL, 
    [Amount] DECIMAL(18, 4) NULL 
)
GO

CREATE NONCLUSTERED INDEX [IX_TopUpPercentage_DateFrom] 
ON [employer_financial].[TopUpPercentage] ([DateFrom] DESC) 
WITH (ONLINE = ON)
GO
