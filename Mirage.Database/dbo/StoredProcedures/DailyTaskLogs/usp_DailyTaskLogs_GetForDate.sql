CREATE PROCEDURE [dbo].[usp_DailyTaskLogs_GetForDate]
    @Date DATE,
    @IsDeleted BIT = 0
AS
BEGIN
    
    SELECT LogID, TaskID, LogDate, Status, CompletedByUserID, CompletedDateTime, Comments, 
           LockOverrideUntil, LockOverrideReason, LockOverrideByUserID, IsDeleted
    FROM [dbo].[DailyTaskLogs]
    WHERE LogDate = @Date 
      AND IsDeleted = @IsDeleted;
END
