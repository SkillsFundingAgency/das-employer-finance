CREATE TABLE [employer_financial].[Payment]
(	
	[PaymentId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
	[Ukprn] BIGINT NOT NULL,	
	[Uln] BIGINT NOT NULL,
    [AccountId] BIGINT NOT NULL,
    [ApprenticeshipId] BIGINT NOT NULL,
    [DeliveryPeriodMonth] INT NOT NULL,
    [DeliveryPeriodYear] INT NOT NULL,
    [CollectionPeriodId] char(20) NOT NULL,
    [CollectionPeriodMonth] int NOT NULL,
    [CollectionPeriodYear] int NOT NULL,
	[EvidenceSubmittedOn] DATETIME NOT NULL,
    [EmployerAccountVersion] VARCHAR(50) NOT NULL,
    [ApprenticeshipVersion] VARCHAR(25) NOT NULL,
    [FundingSource] VARCHAR(25) NOT NULL,
    [TransactionType] VARCHAR(25) NOT NULL,
    [Amount] decimal(18,5) not null default 0,
	[PeriodEnd] VARCHAR(25) not null, 
    [PaymentMetaDataId] BIGINT NOT NULL,     
    [DateImported] DATETIME NULL
	)

GO

CREATE INDEX [IX_Payment_AccountId_Optimized] ON [employer_financial].[Payment] (AccountId) INCLUDE (Ukprn,PeriodEnd,FundingSource,Amount,ApprenticeshipId,Uln,PaymentMetaDataId)
GO

CREATE INDEX [IX_Payment_PaymentMetaDataId] ON [employer_financial].[Payment] ([PaymentMetaDataId])
GO

CREATE INDEX [IX_Payment_FundingComp_Optimized] ON [employer_financial].[Payment] (AccountId, FundingSource, Ukprn, PaymentMetaDataId) INCLUDE (Amount,PeriodEnd,Uln)
GO


CREATE NONCLUSTERED INDEX [IX_Payment_AccountIdUkprnPeriodEndUln] ON [employer_financial].[Payment] ([AccountId], [Ukprn], [PeriodEnd], [Uln]) INCLUDE ([FundingSource], [PaymentMetaDataId]) WITH (ONLINE = ON)
GO

CREATE NONCLUSTERED INDEX [IX_Payment_CollectionPeriod_Optimized] ON [employer_financial].[Payment] ([AccountId], [CollectionPeriodYear], [CollectionPeriodMonth]) INCLUDE ([Amount], [ApprenticeshipId], [PeriodEnd], [Ukprn], [FundingSource], [PaymentMetaDataId]) WITH (ONLINE = ON)
GO

CREATE NONCLUSTERED INDEX [IX_Payment_UkprnAccountId] ON [employer_financial].[Payment] ([AccountId], [Ukprn]) WITH (ONLINE = ON)
GO

CREATE NONCLUSTERED INDEX [IX_Payment_ApprenticeshipId_PeriodEnd] 
ON [employer_financial].[Payment] ([ApprenticeshipId], [PeriodEnd]) 
INCLUDE ([AccountId], [Amount], [FundingSource], [Ukprn])
WITH (ONLINE = ON)
GO

CREATE NONCLUSTERED INDEX [IX_Payment_ULN_AccountId] 
ON [employer_financial].[Payment] ([Uln], [AccountId]) 
INCLUDE ([Amount], [PeriodEnd], [FundingSource])
WITH (ONLINE = ON)
GO

CREATE NONCLUSTERED INDEX [IX_Payment_AccountId_Ukprn_FundingSource] 
ON [employer_financial].[Payment] ([AccountId], [Ukprn], [FundingSource]) 
INCLUDE ([Amount], [PeriodEnd], [PaymentMetaDataId])
WITH (ONLINE = ON)
GO

CREATE NONCLUSTERED INDEX [IX_Payment_PaymentMetaDataId_AccountId] 
ON [employer_financial].[Payment] ([PaymentMetaDataId], [AccountId]) 
INCLUDE ([Amount], [Ukprn], [PeriodEnd])
WITH (ONLINE = ON)
GO