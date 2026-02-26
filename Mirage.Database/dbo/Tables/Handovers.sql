CREATE TABLE [dbo].[Handovers](
    [HandoverID] [int] IDENTITY(1,1) NOT NULL,
    [HandoverNotes] [nvarchar](max) NOT NULL,
    [GivenDateTime] [datetime2](7) NOT NULL DEFAULT GETDATE(),
    [GivenByUserID] [int] NOT NULL,
    [IsReceived] [bit] NOT NULL DEFAULT 0,
    [ReceivedDateTime] [datetime2](7) NULL,
    [ReceivedByUserID] [int] NULL,
    [Priority] [nvarchar](50) NOT NULL DEFAULT 'Normal',
    [Shift] [nvarchar](50) NOT NULL DEFAULT 'N/A',
    [IsActive] [bit] NOT NULL DEFAULT 1,
    [DeactivationReason] [nvarchar](max) NULL,
    [DeactivatedByUserID] [int] NULL,
    [DeactivationDateTime] [datetime2](7) NULL,
    CONSTRAINT [PK_Handovers] PRIMARY KEY CLUSTERED ([HandoverID] ASC),
    CONSTRAINT [FK_Handovers_GivenByUserID] FOREIGN KEY ([GivenByUserID]) REFERENCES [dbo].[Users]([UserID]),
    CONSTRAINT [FK_Handovers_ReceivedByUserID] FOREIGN KEY ([ReceivedByUserID]) REFERENCES [dbo].[Users]([UserID])
)
