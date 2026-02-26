CREATE TABLE [dbo].[DailyTaskLogs](
    [LogID] [bigint] IDENTITY(1,1) NOT NULL,
    [TaskID] [int] NOT NULL,
    [LogDate] [date] NOT NULL,
    [Status] [nvarchar](50) NOT NULL,
    [CompletedByUserID] [int] NULL,
    [CompletedDateTime] [datetime2](7) NULL,
    [Comments] [nvarchar](max) NULL,
    [LockOverrideUntil] [datetime2](7) NULL,
    [LockOverrideReason] [nvarchar](max) NULL,
    [LockOverrideByUserID] [int] NULL,
    CONSTRAINT [PK_DailyTaskLogs] PRIMARY KEY CLUSTERED ([LogID] ASC),
    CONSTRAINT [FK_DailyTaskLogs_Tasks] FOREIGN KEY ([TaskID]) REFERENCES [dbo].[Tasks]([TaskID]) ON DELETE CASCADE,
    CONSTRAINT [FK_DailyTaskLogs_Users] FOREIGN KEY ([CompletedByUserID]) REFERENCES [dbo].[Users]([UserID])
)

GO

CREATE NONCLUSTERED INDEX [IX_DailyTaskLogs_LogDate] ON [dbo].[DailyTaskLogs]([LogDate] ASC)
