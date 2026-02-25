CREATE TABLE [dbo].[KitValidations](
    [ValidationID] [int] IDENTITY(1,1) NOT NULL,
    [KitName] [nvarchar](255) NOT NULL,
    [KitLotNumber] [nvarchar](100) NOT NULL,
    [KitExpiryDate] [date] NOT NULL,
    [ValidationStatus] [nvarchar](50) NOT NULL,
    [Comments] [nvarchar](max) NULL,
    [ValidationDateTime] [datetime2](7) NOT NULL DEFAULT GETDATE(),
    [ValidatedByUserID] [int] NOT NULL,
    [IsActive] [bit] NOT NULL DEFAULT 1,
    [DeactivationReason] [nvarchar](max) NULL,
    [DeactivatedByUserID] [int] NULL,
    [DeactivationDateTime] [datetime2](7) NULL,
    CONSTRAINT [PK_KitValidations] PRIMARY KEY CLUSTERED ([ValidationID] ASC),
    CONSTRAINT [FK_KitValidations_Users] FOREIGN KEY ([ValidatedByUserID]) REFERENCES [dbo].[Users]([UserID])
)
