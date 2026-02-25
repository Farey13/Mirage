CREATE PROCEDURE [dbo].[usp_AuditLog_Create]
    @UserID INT,
    @ActionType NVARCHAR(50),
    @ModuleName NVARCHAR(100),
    @RecordID NVARCHAR(100),
    @FieldName NVARCHAR(100),
    @OldValue NVARCHAR(MAX),
    @NewValue NVARCHAR(MAX)
AS
BEGIN
        SET XACT_ABORT ON;
    INSERT INTO [dbo].[AuditLog] (UserID, ActionType, ModuleName, RecordID, FieldName, OldValue, NewValue)
    VALUES (@UserID, @ActionType, @ModuleName, @RecordID, @FieldName, @OldValue, @NewValue);
END
