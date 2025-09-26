USE MirageDB;
GO

-- Note: We already added IsActive. This script adds the other required columns.
ALTER TABLE SampleStorage
ADD DeactivationReason NVARCHAR(MAX) NULL,
    DeactivatedByUserID INT NULL,
    DeactivationDateTime DATETIME2 NULL;
GO