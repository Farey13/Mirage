USE MirageDB;
GO

ALTER TABLE SampleStorage
ADD TestName NVARCHAR(255) NOT NULL DEFAULT 'N/A'; -- Added a default for existing rows
GO