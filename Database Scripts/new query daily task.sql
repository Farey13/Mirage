USE MirageDB;
GO

-- 1. Create a new table to define shifts
CREATE TABLE Shifts (
    ShiftID INT IDENTITY(1,1) PRIMARY KEY,
    ShiftName NVARCHAR(100) NOT NULL UNIQUE,
    StartTime TIME NOT NULL,
    EndTime TIME NOT NULL,
    GracePeriodHours INT NOT NULL DEFAULT 2,
    IsActive BIT NOT NULL DEFAULT 1
);
GO

-- 2. Update the Tasks table to use the new Shifts table
ALTER TABLE Tasks
ADD ShiftID INT NULL;
GO

ALTER TABLE Tasks
ADD CONSTRAINT FK_Tasks_Shifts FOREIGN KEY (ShiftID) REFERENCES Shifts(ShiftID);
GO

-- (Optional but recommended) Remove the old TaskCategory column after migrating data
-- ALTER TABLE Tasks DROP COLUMN TaskCategory;
-- GO

-- 3. Update the DailyTaskLogs table for the admin override feature
ALTER TABLE DailyTaskLogs
ADD LockOverrideUntil DATETIME2 NULL,
    LockOverrideReason NVARCHAR(MAX) NULL,
    LockOverrideByUserID INT NULL;
GO