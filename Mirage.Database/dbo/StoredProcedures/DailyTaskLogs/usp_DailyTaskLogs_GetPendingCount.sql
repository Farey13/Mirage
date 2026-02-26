CREATE PROCEDURE [dbo].[usp_DailyTaskLogs_GetPendingCount]
    @Date DATE
AS
BEGIN
    
    SELECT COUNT(dtl.LogID)
    FROM [dbo].[DailyTaskLogs] dtl
    INNER JOIN [dbo].[Tasks] t ON dtl.TaskID = t.TaskID
    INNER JOIN [dbo].[Shifts] s ON t.ShiftID = s.ShiftID
    WHERE dtl.LogDate = @Date 
      AND (dtl.Status IS NULL OR dtl.Status NOT IN ('Complete', 'Completed'))
      AND (dtl.LockOverrideUntil IS NULL OR dtl.LockOverrideUntil < GETUTCDATE())
      AND DATEADD(hour, s.GracePeriodHours, CAST(dtl.LogDate AS DATETIME) + CAST(s.EndTime AS DATETIME)) > GETUTCDATE();
END
