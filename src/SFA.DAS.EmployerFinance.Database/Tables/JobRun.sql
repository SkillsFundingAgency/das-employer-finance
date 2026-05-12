CREATE TABLE [employer_financial].[JobRun]
(
    [JobId] NVARCHAR(100) NOT NULL PRIMARY KEY,
    [Description] NVARCHAR(256) NOT NULL,
    [DateStarted] DATETIME2 NOT NULL,
    [NumRecords] INT NOT NULL
)
GO

CREATE NONCLUSTERED INDEX [IX_JobRun_DateStarted] ON [employer_financial].[JobRun] ([DateStarted] DESC)
GO
