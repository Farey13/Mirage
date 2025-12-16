USE MirageDB;
GO

-- 1. Check if the conflicting column 'TaskCategory' exists
IF EXISTS (SELECT * FROM sys.columns WHERE Name = N'TaskCategory' AND Object_ID = Object_ID(N'Tasks'))
BEGIN
    -- Option A: Make it nullable (Safe option, keeps old data)
    ALTER TABLE Tasks ALTER COLUMN TaskCategory NVARCHAR(50) NULL;
    
    -- Option B: Drop it completely (Cleaner, if you don't need old category data)
    -- ALTER TABLE Tasks DROP COLUMN TaskCategory;
END
GO

-- 2. Verification: Check table structure
SELECT * FROM Tasks;
GO