CREATE PROCEDURE [dbo].[usp_Tasks_GetByIds]
    @TaskIds NVARCHAR(MAX)
AS
BEGIN
    
    SELECT TaskID, TaskName, TaskCategory, ScheduleType, ScheduleValue, IsActive, ShiftID
    FROM [dbo].[Tasks]
    WHERE TaskID IN (SELECT CAST(value AS INT) FROM STRING_SPLIT(@TaskIds, ','));
END
