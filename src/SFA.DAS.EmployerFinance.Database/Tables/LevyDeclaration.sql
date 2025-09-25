CREATE TABLE [employer_financial].[LevyDeclaration]
(
	[Id] BIGINT NOT NULL PRIMARY KEY IDENTITY, 
	[AccountId] BIGINT NOT NULL DEFAULT 0,
    [EmpRef] NVARCHAR(50) NOT NULL, 
    [LevyDueYTD] DECIMAL(18, 4) NULL DEFAULT 0, 
    [LevyAllowanceForYear] DECIMAL(18, 4) NULL DEFAULT 0, 
    [SubmissionDate] DATETIME NULL, 
    [SubmissionId] BIGINT NOT NULL DEFAULT 0,
	[PayrollYear] NVARCHAR(10) NULL,
	[PayrollMonth] TINYINT NULL,
	[CreatedDate] DATETIME NOT NULL DEFAULT GetDate(),
	[EndOfYearAdjustment] BIT NOT NULL DEFAULT 0,
	[EndOfYearAdjustmentAmount] DECIMAL(18,4) NULL,
	[DateCeased] DATETIME NULL,
	[InactiveFrom] DATETIME NULL,
	[InactiveTo] DATETIME NULL,
	[HmrcSubmissionId] BIGINT NULL,
	[NoPaymentForPeriod] BIT DEFAULT 0
)

GO

CREATE INDEX [IX_LevyDeclaration_EmpRef] 
ON [employer_financial].[LevyDeclaration] (EmpRef,PayrollYear,PayrollMonth,EndOfYearAdjustment) 
GO

CREATE INDEX [IX_LevyDeclaration_Account_Payroll_Optimized]
ON [employer_financial].[LevyDeclaration] (AccountId,PayrollMonth,PayrollYear) INCLUDE (LevyDueYTD,EmpRef,SubmissionId,SubmissionDate,EndOfYearAdjustment)
GO

CREATE UNIQUE NONCLUSTERED INDEX [IDX_UNIQUE_LevyDeclaration_SubmissionId]
ON [employer_financial].[LevyDeclaration] (SubmissionId)
GO

CREATE NONCLUSTERED INDEX [IX_LevyDeclaration_EmpRef_EOYAdjustment_Optimized]
ON [employer_financial].[LevyDeclaration] ([empRef],[EndOfYearAdjustment]) INCLUDE ([SubmissionDate],[PayrollYear],[PayrollMonth],[AccountId],[LevyDueYTD],[SubmissionId])
GO

CREATE NONCLUSTERED INDEX [IX_LevyDeclaration_AccountId_EmpRef] 
ON [employer_financial].[LevyDeclaration] ([AccountId], [EmpRef]) 
INCLUDE ([SubmissionId], [LevyDueYTD], [PayrollYear], [PayrollMonth])
WITH (ONLINE = ON)
GO

CREATE NONCLUSTERED INDEX [IX_LevyDeclaration_SubmissionId_AccountId] 
ON [employer_financial].[LevyDeclaration] ([SubmissionId], [AccountId]) 
INCLUDE ([EmpRef], [LevyDueYTD], [PayrollYear], [PayrollMonth])
WITH (ONLINE = ON)
GO

CREATE NONCLUSTERED INDEX [IX_LevyDeclaration_AccountId_SubmissionDate] 
ON [employer_financial].[LevyDeclaration] ([AccountId], [SubmissionDate] DESC) 
INCLUDE ([EmpRef], [LevyDueYTD], [PayrollYear], [PayrollMonth], [EndOfYearAdjustment])
WITH (ONLINE = ON)
GO