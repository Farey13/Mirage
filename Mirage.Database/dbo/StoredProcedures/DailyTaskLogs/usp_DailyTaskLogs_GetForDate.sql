CREATE PROCEDURE [dbo].[usp_DailyTaskLogs_GetForDate]
    @Date DATE
AS
BEGIN
    
    SELECT LogID, TaskID, LogDate, Status, CompletedByUserID, CompletedDateTime, Comments, 
           LockOverrideUntil, LockOverrideReason, LockOverrideByUserID
    FROM [dbo].[DailyTaskLogs]
    WHERE LogDate = @Date;
END
