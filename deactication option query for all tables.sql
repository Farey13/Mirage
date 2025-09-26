USE MirageDB;
GO

-- Add columns to CalibrationLogs
IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = N'IsActive' AND Object_ID = Object_ID(N'CalibrationLogs'))
BEGIN
    ALTER TABLE CalibrationLogs
    ADD IsActive BIT NOT NULL DEFAULT 1,
        DeactivationReason NVARCHAR(MAX) NULL,
        DeactivatedByUserID INT NULL,
        DeactivationDateTime DATETIME2 NULL;
END
GO

-- Add columns to KitValidations
IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = N'IsActive' AND Object_ID = Object_ID(N'KitValidations'))
BEGIN
    ALTER TABLE KitValidations
    ADD IsActive BIT NOT NULL DEFAULT 1,
        DeactivationReason NVARCHAR(MAX) NULL,
        DeactivatedByUserID INT NULL,
        DeactivationDateTime DATETIME2 NULL;
END
GO

-- Add columns to Handovers
IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = N'IsActive' AND Object_ID = Object_ID(N'Handovers'))
BEGIN
    ALTER TABLE Handovers
    ADD IsActive BIT NOT NULL DEFAULT 1,
        DeactivationReason NVARCHAR(MAX) NULL,
        DeactivatedByUserID INT NULL,
        DeactivationDateTime DATETIME2 NULL;
END
GO

-- Add columns to MachineBreakdowns
IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = N'IsActive' AND Object_ID = Object_ID(N'MachineBreakdowns'))
BEGIN
    ALTER TABLE MachineBreakdowns
    ADD IsActive BIT NOT NULL DEFAULT 1,
        DeactivationReason NVARCHAR(MAX) NULL,
        DeactivatedByUserID INT NULL,
        DeactivationDateTime DATETIME2 NULL;
END
GO