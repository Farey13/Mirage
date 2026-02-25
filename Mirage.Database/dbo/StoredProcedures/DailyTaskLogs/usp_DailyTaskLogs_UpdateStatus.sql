CREATE PROCEDURE [dbo].[usp_DailyTaskLogs_UpdateStatus]
    @LogId BIGINT,
    @Status NVARCHAR(50),
    @UserId INT,
    @Comment NVARCHAR(MAX)
AS
BEGIN
        SET XACT_ABORT ON;
    DECLARE @CompletedTime DATETIME2(7) = NULL;
    IF @Status = 'Completed'
    BEGIN
        SET @CompletedTime = GETUTCDATE();
    END
    UPDATE [dbo].[DailyTaskLogs]
    SET Status = @Status, 
        CompletedByUserID = @UserId, 
        CompletedDateTime = @CompletedTime,
        Comments = @Comment
    OUTPUT INSERTED.LogID, INSERTED.TaskID, INSERTED.LogDate, INSERTED.Status, INSERTED.CompletedByUserID, 
           INSERTED.CompletedDateTime, INSERTED.Comments, INSERTED.LockOverrideUntil, INSERTED.LockOverrideReason, INSERTED.LockOverrideByUserID
    WHERE LogID = @LogId;
END
