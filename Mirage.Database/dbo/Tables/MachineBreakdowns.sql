CREATE TABLE [dbo].[MachineBreakdowns](
    [BreakdownID] [int] IDENTITY(1,1) NOT NULL,
    [MachineName] [nvarchar](255) NOT NULL,
    [BreakdownReason] [nvarchar](max) NOT NULL,
    [ReportedDateTime] [datetime2](7) NOT NULL DEFAULT GETDATE(),
    [ReportedByUserID] [int] NOT NULL,
    [IsResolved] [bit] NOT NULL DEFAULT 0,
    [ResolvedDateTime] [datetime2](7) NULL,
    [ResolvedByUserID] [int] NULL,
    [ResolutionNotes] [nvarchar](max) NULL,
    [DowntimeMinutes] [int] NULL,
    [IsActive] [bit] NOT NULL DEFAULT 1,
    [DeactivationReason] [nvarchar](max) NULL,
    [DeactivatedByUserID] [int] NULL,
    [DeactivationDateTime] [datetime2](7) NULL,
    CONSTRAINT [PK_MachineBreakdowns] PRIMARY KEY CLUSTERED ([BreakdownID] ASC),
    CONSTRAINT [FK_MachineBreakdowns_ReportedByUserID] FOREIGN KEY ([ReportedByUserID]) REFERENCES [dbo].[Users]([UserID]),
    CONSTRAINT [FK_MachineBreakdowns_ResolvedByUserID] FOREIGN KEY ([ResolvedByUserID]) REFERENCES [dbo].[Users]([UserID])
)

GO

CREATE NONCLUSTERED INDEX [IX_MachineBreakdowns_ReportedDateTime] ON [dbo].[MachineBreakdowns]([ReportedDateTime] ASC)
