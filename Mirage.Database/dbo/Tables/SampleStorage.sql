CREATE TABLE [dbo].[SampleStorage](
    [StorageID] [int] IDENTITY(1,1) NOT NULL,
    [PatientSampleID] [nvarchar](100) NOT NULL,
    [StorageDateTime] [datetime2](7) NOT NULL DEFAULT GETDATE(),
    [StoredByUserID] [int] NOT NULL,
    [IsTestDone] [bit] NOT NULL DEFAULT 0,
    [TestDoneDateTime] [datetime2](7) NULL,
    [TestDoneByUserID] [int] NULL,
    [TestName] [nvarchar](255) NOT NULL DEFAULT 'N/A',
    [IsActive] [bit] NOT NULL DEFAULT 1,
    [DeactivationReason] [nvarchar](max) NULL,
    [DeactivatedByUserID] [int] NULL,
    [DeactivationDateTime] [datetime2](7) NULL,
    CONSTRAINT [PK_SampleStorage] PRIMARY KEY CLUSTERED ([StorageID] ASC),
    CONSTRAINT [FK_SampleStorage_StoredByUserID] FOREIGN KEY ([StoredByUserID]) REFERENCES [dbo].[Users]([UserID]),
    CONSTRAINT [FK_SampleStorage_TestDoneByUserID] FOREIGN KEY ([TestDoneByUserID]) REFERENCES [dbo].[Users]([UserID])
)
