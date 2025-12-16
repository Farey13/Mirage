USE MirageDB;
GO

-- 1. Force all existing shifts to be Active (Visible)
UPDATE Shifts
SET IsActive = 1
WHERE IsActive IS NULL OR IsActive = 0;

-- 2. Ensure GracePeriodHours has a valid value (required by C# code)
UPDATE Shifts
SET GracePeriodHours = 2
WHERE GracePeriodHours IS NULL;

-- 3. Verify the data is now correct
SELECT * FROM Shifts;
GO