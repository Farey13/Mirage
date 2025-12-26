USE MirageDB;
GO

-- 1. Add 'GracePeriodHours' column if it is missing
IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = N'GracePeriodHours' AND Object_ID = Object_ID(N'Shifts'))
BEGIN
    ALTER TABLE Shifts ADD GracePeriodHours INT NOT NULL DEFAULT 2;
END
GO

-- 2. Add 'IsActive' column if it is missing
IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = N'IsActive' AND Object_ID = Object_ID(N'Shifts'))
BEGIN
    ALTER TABLE Shifts ADD IsActive BIT NOT NULL DEFAULT 1;
END
GO

-- 3. FIX THE DATA: Make sure old shifts are visible
-- This updates any record where IsActive is NULL or 0 to be 1 (Active)
UPDATE Shifts
SET IsActive = 1
WHERE IsActive IS NULL OR IsActive = 0;

-- 4. FIX THE DATA: Ensure Grace Period has a valid value
UPDATE Shifts
SET GracePeriodHours = 2
WHERE GracePeriodHours IS NULL;
GO

-- 5. Verification: Show the fixed data
SELECT * FROM Shifts;
GO