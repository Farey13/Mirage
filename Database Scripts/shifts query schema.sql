USE MirageDB;
GO

-- 1. Check if Shifts table exists; if NOT, create it entirely
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Shifts')
BEGIN
    CREATE TABLE Shifts (
        ShiftID INT IDENTITY(1,1) PRIMARY KEY,
        ShiftName NVARCHAR(100) NOT NULL UNIQUE,
        StartTime TIME NOT NULL,
        EndTime TIME NOT NULL,
        GracePeriodHours INT NOT NULL DEFAULT 2,
        IsActive BIT NOT NULL DEFAULT 1
    );
END
ELSE
BEGIN
    -- 2. If table EXISTS, check for missing columns and add them
    
    -- Check for GracePeriodHours
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = N'GracePeriodHours' AND Object_ID = Object_ID(N'Shifts'))
    BEGIN
        ALTER TABLE Shifts ADD GracePeriodHours INT NOT NULL DEFAULT 2;
    END

    -- Check for IsActive
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = N'IsActive' AND Object_ID = Object_ID(N'Shifts'))
    BEGIN
        ALTER TABLE Shifts ADD IsActive BIT NOT NULL DEFAULT 1;
    END
END
GO