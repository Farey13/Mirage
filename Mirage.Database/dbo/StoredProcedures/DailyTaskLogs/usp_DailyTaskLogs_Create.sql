CREATE PROCEDURE [dbo].[usp_DailyTaskLogs_Create]
    @TaskID INT,
    @LogDate DATE,
    @Status NVARCHAR(50),
    @CompletedByUserID INT,
    @CompletedDateTime DATETIME2(7),
    @Comments NVARCHAR(MAX)
AS
BEGIN
        SET XACT_ABORT ON;
    INSERT INTO [dbo].[DailyTaskLogs] (TaskID, LogDate, Status, CompletedByUserID, CompletedDateTime, Comments)
    OUTPUT INSERTED.LogID, INSERTED.TaskID, INSERTED.LogDate, INSERTED.Status, INSERTED.CompletedByUserID, 
           INSERTED.CompletedDateTime, INSERTED.Comments, INSERTED.LockOverrideUntil, INSERTED.LockOverrideReason, INSERTED.LockOverrideByUserID
    VALUES (@TaskID, @LogDate, @Status, @CompletedByUserID, @CompletedDateTime, @Comments);
END
