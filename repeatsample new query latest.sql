USE MirageDB;
GO

CREATE TABLE RepeatSampleLog (
    RepeatID INT IDENTITY(1,1) PRIMARY KEY,
    PatientIdCardNumber NVARCHAR(100),
    PatientName NVARCHAR(255) NOT NULL,
    ReasonText NVARCHAR(MAX),
    InformedPerson NVARCHAR(255),
    Department NVARCHAR(50),
    LogDateTime DATETIME2 NOT NULL DEFAULT GETDATE(),
    LoggedByUserID INT NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    DeactivationReason NVARCHAR(MAX) NULL,
    DeactivatedByUserID INT NULL,
    DeactivationDateTime DATETIME2 NULL,
    FOREIGN KEY (LoggedByUserID) REFERENCES Users(UserID)
);
GO