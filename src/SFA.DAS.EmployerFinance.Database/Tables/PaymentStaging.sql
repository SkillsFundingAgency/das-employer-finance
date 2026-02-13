CREATE TABLE [employer_financial].[PaymentStaging]
(
	[PaymentId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, 
    [Ukprn] BIGINT NOT NULL, 
    [Uln] BIGINT NOT NULL, 
    [AccountId] BIGINT NOT NULL, 
    [ApprenticeshipId] BIGINT NOT NULL, 
    [DeliveryPeriodMonth] INT NOT NULL, 
    [DeliveryPeriodYear] INT NOT NULL, 
    [CollectionPeriodId] CHAR(20) NOT NULL, 
    [CollectionPeriodMonth] INT NOT NULL, 
    [CollectionPeriodYear] INT NOT NULL, 
    [FundingSource] VARCHAR(25) NOT NULL, 
    [TransactionType] VARCHAR(25) NOT NULL, 
    [Amount] DECIMAL(18, 5) NOT NULL, 
    [EvidenceSubmittedOn] DATETIME NOT NULL, 
    [EmployerAccountVersion] VARCHAR(50) NOT NULL, 
    [ApprenticeshipVersion] VARCHAR(25) NOT NULL, 
    [ProcessingStatus] VARCHAR(50) NULL, 
    [TimeStamps] DATETIME NULL, 
    [MetaDataJson] NVARCHAR(MAX) NULL
)
 
GO
 
CREATE NONCLUSTERED INDEX [IX_PaymentStaging_CollectionPeriodID_AccountId] ON [employer_financial].[PaymentStaging] (AccountId, CollectionPeriodId)
 
GO
 
CREATE NONCLUSTERED INDEX [IX_PaymentStaging_ProcessingStatus] ON [employer_financial].[PaymentStaging] (ProcessingStatus)
