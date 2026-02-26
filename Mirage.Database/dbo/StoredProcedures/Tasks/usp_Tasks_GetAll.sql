CREATE PROCEDURE [dbo].[usp_Tasks_GetAll]
AS
BEGIN
    
    SELECT TaskID, TaskName, TaskCategory, ScheduleType, ScheduleValue, IsActive, ShiftID
    FROM [dbo].[Tasks]
    WHERE IsActive = 1;
END
