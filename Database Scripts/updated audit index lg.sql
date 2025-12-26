USE MirageDB;
GO

-- Create an index on the Timestamp column to keep queries fast
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AuditLog_Timestamp' AND object_id = OBJECT_ID('dbo.AuditLog'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_AuditLog_Timestamp
    ON dbo.AuditLog (Timestamp DESC);
END
GO