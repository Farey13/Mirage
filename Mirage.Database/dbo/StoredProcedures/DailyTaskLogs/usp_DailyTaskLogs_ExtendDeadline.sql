CREATE PROCEDURE [dbo].[usp_DailyTaskLogs_ExtendDeadline]
    @LogId BIGINT,
    @NewDeadline DATETIME2(7),
    @Reason NVARCHAR(MAX),
    @AdminUserId INT
AS
BEGIN
        SET XACT_ABORT ON;
    UPDATE [dbo].[DailyTaskLogs]
    SET Status = 'Pending',
        LockOverrideUntil = @NewDeadline,
        LockOverrideReason = @Reason,
        LockOverrideByUserID = @AdminUserId
    OUTPUT INSERTED.LogID, INSERTED.TaskID, INSERTED.LogDate, INSERTED.Status, INSERTED.CompletedByUserID, 
           INSERTED.CompletedDateTime, INSERTED.Comments, INSERTED.LockOverrideUntil, INSERTED.LockOverrideReason, INSERTED.LockOverrideByUserID
    WHERE LogID = @LogId;
END
