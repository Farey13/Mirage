USE MirageDB;
GO

ALTER TABLE MediaSterilityChecks
ADD IsActive BIT NOT NULL DEFAULT 1,
    DeactivationReason NVARCHAR(MAX) NULL,
    DeactivatedByUserID INT NULL,
    DeactivationDateTime DATETIME2 NULL;
GO