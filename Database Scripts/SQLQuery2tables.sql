-- Select the correct database before running the script
USE MirageDB;
GO

-- =================================================================
-- Phase 1, Step 1.1 (Part 2): Logbook Module Tables
-- =================================================================

-- 1. Repeat Sample Book
CREATE TABLE RepeatSampleLog (
    RepeatID INT IDENTITY(1,1) PRIMARY KEY,
    PatientIdCardNumber NVARCHAR(100),
    PatientName NVARCHAR(255) NOT NULL,
    InformedPersonOrDept NVARCHAR(255),
    ReasonText NVARCHAR(MAX),
    LogDateTime DATETIME2 NOT NULL DEFAULT GETDATE(),
    LoggedByUserID INT NOT NULL,
    FOREIGN KEY (LoggedByUserID) REFERENCES Users(UserID)
);
GO

-- 2. Kit Validation Book
CREATE TABLE KitValidations (
    ValidationID INT IDENTITY(1,1) PRIMARY KEY,
    KitName NVARCHAR(255) NOT NULL,
    KitLotNumber NVARCHAR(100) NOT NULL,
    KitExpiryDate DATE NOT NULL,
    ValidationStatus NVARCHAR(50) NOT NULL, -- "Accepted", "Rejected"
    Comments NVARCHAR(MAX),
    ValidationDateTime DATETIME2 NOT NULL DEFAULT GETDATE(),
    ValidatedByUserID INT NOT NULL,
    FOREIGN KEY (ValidatedByUserID) REFERENCES Users(UserID)
);
GO

-- 3. Calibration Log
CREATE TABLE CalibrationLogs (
    CalibrationID INT IDENTITY(1,1) PRIMARY KEY,
    TestName NVARCHAR(255) NOT NULL,
    QcResult NVARCHAR(50) NOT NULL, -- "Passed", "Failed"
    Reason NVARCHAR(MAX),
    CalibrationDateTime DATETIME2 NOT NULL DEFAULT GETDATE(),
    PerformedByUserID INT NOT NULL,
    FOREIGN KEY (PerformedByUserID) REFERENCES Users(UserID)
);
GO

-- 4. Sample Storage Book
CREATE TABLE SampleStorage (
    StorageID INT IDENTITY(1,1) PRIMARY KEY,
    PatientSampleID NVARCHAR(100) NOT NULL,
    StorageDateTime DATETIME2 NOT NULL DEFAULT GETDATE(),
    StoredByUserID INT NOT NULL,
    IsTestDone BIT NOT NULL DEFAULT 0,
    TestDoneDateTime DATETIME2 NULL,
    TestDoneByUserID INT NULL,
    FOREIGN KEY (StoredByUserID) REFERENCES Users(UserID),
    FOREIGN KEY (TestDoneByUserID) REFERENCES Users(UserID)
);
GO

-- 5. Handover Book
CREATE TABLE Handovers (
    HandoverID INT IDENTITY(1,1) PRIMARY KEY,
    HandoverNotes NVARCHAR(MAX) NOT NULL,
    GivenDateTime DATETIME2 NOT NULL DEFAULT GETDATE(),
    GivenByUserID INT NOT NULL,
    IsReceived BIT NOT NULL DEFAULT 0,
    ReceivedDateTime DATETIME2 NULL,
    ReceivedByUserID INT NULL,
    FOREIGN KEY (GivenByUserID) REFERENCES Users(UserID),
    FOREIGN KEY (ReceivedByUserID) REFERENCES Users(UserID)
);
GO

-- 6. Machine Breakdown Log
CREATE TABLE MachineBreakdowns (
    BreakdownID INT IDENTITY(1,1) PRIMARY KEY,
    MachineName NVARCHAR(255) NOT NULL,
    BreakdownReason NVARCHAR(MAX) NOT NULL,
    ReportedDateTime DATETIME2 NOT NULL DEFAULT GETDATE(),
    ReportedByUserID INT NOT NULL,
    IsResolved BIT NOT NULL DEFAULT 0,
    ResolvedDateTime DATETIME2 NULL,
    ResolvedByUserID INT NULL,
    FOREIGN KEY (ReportedByUserID) REFERENCES Users(UserID),
    FOREIGN KEY (ResolvedByUserID) REFERENCES Users(UserID)
);
GO

-- 7. Media Sterility Book
CREATE TABLE MediaSterilityChecks (
    SterilityCheckID INT IDENTITY(1,1) PRIMARY KEY,
    MediaName NVARCHAR(255) NOT NULL,
    MediaLotNumber NVARCHAR(100) NOT NULL,
    MediaQuantity NVARCHAR(100),
    Result37C NVARCHAR(50) NOT NULL, -- "No Growth", "Growth Seen"
    Result25C NVARCHAR(50) NOT NULL, -- "No Growth", "Growth Seen"
    OverallStatus NVARCHAR(50) NOT NULL, -- "Passed", "Failed"
    Comments NVARCHAR(MAX),
    CheckDateTime DATETIME2 NOT NULL DEFAULT GETDATE(),
    PerformedByUserID INT NOT NULL,
    FOREIGN KEY (PerformedByUserID) REFERENCES Users(UserID)
);
GO

-- 8. Daily Task Log (Part 1 - The Task Definitions)
CREATE TABLE Tasks (
    TaskID INT IDENTITY(1,1) PRIMARY KEY,
    TaskName NVARCHAR(255) NOT NULL,
    TaskCategory NVARCHAR(50) NOT NULL, -- "Morning", "Evening"
    ScheduleType NVARCHAR(50) NOT NULL, -- "Daily", "Weekly", "Monthly"
    ScheduleValue NVARCHAR(50) NULL, -- For Weekly: "Monday", "Tuesday", etc. For Monthly: "1"-"31"
    IsActive BIT NOT NULL DEFAULT 1
);
GO

-- 9. Daily Task Log (Part 2 - The Daily Records)
CREATE TABLE DailyTaskLogs (
    LogID BIGINT IDENTITY(1,1) PRIMARY KEY,
    TaskID INT NOT NULL,
    LogDate DATE NOT NULL,
    Status NVARCHAR(50) NOT NULL, -- "Pending", "Completed", "Not Available"
    CompletedByUserID INT NULL,
    CompletedDateTime DATETIME2 NULL,
    FOREIGN KEY (TaskID) REFERENCES Tasks(TaskID) ON DELETE CASCADE,
    FOREIGN KEY (CompletedByUserID) REFERENCES Users(UserID)
);
GO