CREATE TABLE [dbo].[AuditLog](
    [AuditID] [bigint] IDENTITY(1,1) NOT NULL,
    [UserID] [int] NULL,
    [Timestamp] [datetime2](7) NOT NULL DEFAULT GETDATE(),
    [ActionType] [nvarchar](50) NOT NULL,
    [ModuleName] [nvarchar](100) NULL,
    [RecordID] [nvarchar](100) NULL,
    [FieldName] [nvarchar](100) NULL,
    [OldValue] [nvarchar](max) NULL,
    [NewValue] [nvarchar](max) NULL,
    CONSTRAINT [PK_AuditLog] PRIMARY KEY CLUSTERED ([AuditID] ASC),
    CONSTRAINT [FK_AuditLog_Users] FOREIGN KEY ([UserID]) REFERENCES [dbo].[Users]([UserID]) ON DELETE SET NULL
)


GO

CREATE INDEX [IX_AuditLog_Timestamp] ON [dbo].[AuditLog]([Timestamp] DESC)
