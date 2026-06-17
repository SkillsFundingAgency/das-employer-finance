CREATE TABLE [employer_financial].[WorkflowLog]
(
    [Id] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [JobId] NVARCHAR(100) NOT NULL,
    [WorkflowId] NVARCHAR(200) NOT NULL,
    [SpanId] NVARCHAR(200) NOT NULL,
    [Sequence] INT NOT NULL,
    [Stage] NVARCHAR(100) NOT NULL,
    [Description] NVARCHAR(512) NOT NULL,
    [Status] NVARCHAR(50) NOT NULL,
    [DataJson] NVARCHAR(MAX) NULL,
    [ErrorCode] NVARCHAR(100) NULL,
    [ErrorMessage] NVARCHAR(4000) NULL,
    [Created] DATETIME2 NOT NULL,
    CONSTRAINT [FK_WorkflowLog_JobRun] FOREIGN KEY ([JobId]) REFERENCES [employer_financial].[JobRun]([JobId])
)
GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_WorkflowLog_WorkflowId_Sequence] ON [employer_financial].[WorkflowLog] ([WorkflowId], [Sequence])
GO

CREATE NONCLUSTERED INDEX [IX_WorkflowLog_JobId] ON [employer_financial].[WorkflowLog] ([JobId])
GO

CREATE NONCLUSTERED INDEX [IX_WorkflowLog_JobId_WorkflowId] ON [employer_financial].[WorkflowLog] ([JobId], [WorkflowId])
GO
