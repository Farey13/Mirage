CREATE TABLE [dbo].[MediaSterilityChecks](
    [SterilityCheckID] [int] IDENTITY(1,1) NOT NULL,
    [MediaName] [nvarchar](255) NOT NULL,
    [MediaLotNumber] [nvarchar](100) NOT NULL,
    [MediaQuantity] [nvarchar](100) NULL,
    [Result37C] [nvarchar](50) NOT NULL,
    [Result25C] [nvarchar](50) NOT NULL,
    [OverallStatus] [nvarchar](50) NOT NULL,
    [Comments] [nvarchar](max) NULL,
    [CheckDateTime] [datetime2](7) NOT NULL DEFAULT GETDATE(),
    [PerformedByUserID] [int] NOT NULL,
    [IsActive] [bit] NOT NULL DEFAULT 1,
    [DeactivationReason] [nvarchar](max) NULL,
    [DeactivatedByUserID] [int] NULL,
    [DeactivationDateTime] [datetime2](7) NULL,
    CONSTRAINT [PK_MediaSterilityChecks] PRIMARY KEY CLUSTERED ([SterilityCheckID] ASC),
    CONSTRAINT [FK_MediaSterilityChecks_Users] FOREIGN KEY ([PerformedByUserID]) REFERENCES [dbo].[Users]([UserID])
)
