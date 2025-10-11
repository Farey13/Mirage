USE MirageDB;
GO

-- First, drop the table if it exists from a partial creation
IF OBJECT_ID('dbo.AdminListItems', 'U') IS NOT NULL
  DROP TABLE dbo.AdminListItems;
GO

-- Now, create the table with all the correct columns
CREATE TABLE AdminListItems (
    ItemID INT IDENTITY(1,1) PRIMARY KEY,
    ListType NVARCHAR(100) NOT NULL,
    ItemValue NVARCHAR(255) NOT NULL,
    Description NVARCHAR(500) NULL,
    IsActive BIT NOT NULL DEFAULT 1
);
GO