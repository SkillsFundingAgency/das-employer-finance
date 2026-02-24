CREATE TABLE [employer_financial].[PaymentMetaDataStaging]
(
    [Id] BIGINT IDENTITY(1,1) NOT NULL,
    [PaymentId] UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT [DF_PaymentMetaDataStaging_PaymentId] DEFAULT ('00000000-0000-0000-0000-000000000000'),
    [ProviderName] VARCHAR(MAX) NULL,
    [StandardCode] BIGINT NULL, 
    [FrameworkCode] INT NULL,     
    [ProgrammeType] INT NULL, 
    [PathwayCode] INT NULL, 
    [PathwayName] VARCHAR(MAX) NULL,
    [ApprenticeshipCourseName] VARCHAR(MAX) NULL, 
    [ApprenticeshipCourseStartDate] DATETIME NULL, 
    [ApprenticeshipCourseLevel] INT NULL, 
    [ApprenticeName] VARCHAR(MAX) NULL, 
    [ApprenticeNINumber] VARCHAR(MAX) NULL, 
    [IsHistoricProviderName] BIT NOT NULL
        CONSTRAINT [DF_PaymentMetaDataStaging_IsHistoricProviderName] DEFAULT (0),
    [CreatedBy] NCHAR(30) NULL, 
    [CorrelationId] UNIQUEIDENTIFIER NULL,
    CONSTRAINT [PK_PaymentMetaDataStaging] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_PaymentMetaDataStaging_PaymentStaging]
        FOREIGN KEY ([PaymentId])
        REFERENCES [employer_financial].[PaymentStaging] ([PaymentId])
)
GO

CREATE NONCLUSTERED INDEX [IX_PaymentMetaDataStaging_PaymentId]
ON [employer_financial].[PaymentMetaDataStaging] ([PaymentId])
GO