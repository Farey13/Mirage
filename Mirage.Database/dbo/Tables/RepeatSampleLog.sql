CREATE TABLE [dbo].[RepeatSampleLog](
    [RepeatID] [int] IDENTITY(1,1) NOT NULL,
    [PatientIdCardNumber] [nvarchar](100) NULL,
    [PatientName] [nvarchar](255) NOT NULL,
    [ReasonText] [nvarchar](max) NULL,
    [InformedPerson] [nvarchar](255) NULL,
    [Department] [nvarchar](50) NULL,
    [LogDateTime] [datetime2](7) NOT NULL DEFAULT GETDATE(),
    [LoggedByUserID] [int] NOT NULL,
    [IsActive] [bit] NOT NULL DEFAULT 1,
    [DeactivationReason] [nvarchar](max) NULL,
    [DeactivatedByUserID] [int] NULL,
    [DeactivationDateTime] [datetime2](7) NULL,
    CONSTRAINT [PK_RepeatSampleLog] PRIMARY KEY CLUSTERED ([RepeatID] ASC),
    CONSTRAINT [FK_RepeatSampleLog_Users] FOREIGN KEY ([LoggedByUserID]) REFERENCES [dbo].[Users]([UserID])
)
