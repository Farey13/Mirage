CREATE TABLE [dbo].[CalibrationLogs](
    [CalibrationID] [int] IDENTITY(1,1) NOT NULL,
    [TestName] [nvarchar](255) NOT NULL,
    [QcResult] [nvarchar](50) NOT NULL,
    [Reason] [nvarchar](max) NULL,
    [CalibrationDateTime] [datetime2](7) NOT NULL DEFAULT GETDATE(),
    [PerformedByUserID] [int] NOT NULL,
    [IsActive] [bit] NOT NULL DEFAULT 1,
    [DeactivationReason] [nvarchar](max) NULL,
    [DeactivatedByUserID] [int] NULL,
    [DeactivationDateTime] [datetime2](7) NULL,
    CONSTRAINT [PK_CalibrationLogs] PRIMARY KEY CLUSTERED ([CalibrationID] ASC),
    CONSTRAINT [FK_CalibrationLogs_Users] FOREIGN KEY ([PerformedByUserID]) REFERENCES [dbo].[Users]([UserID])
)
