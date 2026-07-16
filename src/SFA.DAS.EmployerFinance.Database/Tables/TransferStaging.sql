CREATE TABLE [employer_financial].[TransferStaging]
(
    TransferId BIGINT NOT NULL,

    SenderAccountId BIGINT NOT NULL,
    SenderAccountName NVARCHAR(100) NOT NULL
        CONSTRAINT DF_TransferStaging_SenderAccountName DEFAULT (''),
    ReceiverAccountId BIGINT NOT NULL,
    ReceiverAccountName NVARCHAR(200) NOT NULL,
    Amount DECIMAL(18, 2) NOT NULL,
    TransferDate DATETIME2(0) NOT NULL,

    PeriodEnd VARCHAR(25) NOT NULL,
    CollectionPeriodMonth INT NOT NULL,
    CollectionPeriodYear INT NOT NULL,

    Ukprn BIGINT NOT NULL,
    CourseName NVARCHAR(200) NULL,
    CourseLevel INT NULL,
    LearningType VARCHAR(25) NULL,

    ApprenticeshipId BIGINT NOT NULL
        CONSTRAINT DF_TransferStaging_ApprenticeshipId DEFAULT (0),
    [Type] NVARCHAR(50) NOT NULL
        CONSTRAINT DF_TransferStaging_Type DEFAULT (''),
    RequiredPaymentId UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT DF_TransferStaging_RequiredPaymentId DEFAULT ('00000000-0000-0000-0000-000000000000'),

    CreatedAt DATETIME2(0) NOT NULL 
        CONSTRAINT DF_TransferStaging_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CreatedBy NVARCHAR(100) NOT NULL,
    CorrelationId NVARCHAR(100) NULL,

    ProcessingStatus TINYINT NOT NULL 
        CONSTRAINT DF_TransferStaging_ProcessingStatus DEFAULT (0),

    CONSTRAINT PK_TransferStaging PRIMARY KEY CLUSTERED (TransferId)
);
