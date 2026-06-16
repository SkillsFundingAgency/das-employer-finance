CREATE TABLE [employer_financial].[TransactionLineStaging]

(

    [Id] BIGINT NOT NULL PRIMARY KEY IDENTITY(1,1),

    [AccountId] BIGINT NOT NULL,

    [DateCreated] DATETIME NOT NULL,

    [SubmissionId] BIGINT NULL,

    [TransactionDate] DATETIME NOT NULL,

    [TransactionType] TINYINT NOT NULL CONSTRAINT [DF_TransactionLineStaging_TransactionType] DEFAULT 0,

    [LevyDeclared] DECIMAL(18,4) NULL,

    [Amount] DECIMAL(18,4) NOT NULL CONSTRAINT [DF_TransactionLineStaging_Amount] DEFAULT 0,

    [EmpRef] NVARCHAR(50) NULL,

    [PeriodEnd] NVARCHAR(50) NULL,

    [Ukprn] BIGINT NULL,

    [SfaCoInvestmentAmount] DECIMAL(18,4) NOT NULL CONSTRAINT [DF_TransactionLineStaging_SfaCoInvestmentAmount] DEFAULT 0,

    [EmployerCoInvestmentAmount] DECIMAL(18,4) NOT NULL CONSTRAINT [DF_TransactionLineStaging_EmployerCoInvestmentAmount] DEFAULT 0,

    [EnglishFraction] DECIMAL(18,5) NULL,

    [TransferSenderAccountId] BIGINT NULL,

    [TransferSenderAccountName] NVARCHAR(100) NULL,

    [TransferReceiverAccountId] BIGINT NULL,

    [TransferReceiverAccountName] NVARCHAR(100) NULL

)

GO
 
CREATE NONCLUSTERED INDEX [IX_TransactionLineStaging_AccountId] ON [employer_financial].[TransactionLineStaging] ([AccountId])

GO

 