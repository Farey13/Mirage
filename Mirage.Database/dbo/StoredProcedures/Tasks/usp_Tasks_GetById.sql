CREATE PROCEDURE [dbo].[usp_Tasks_GetById]
    @TaskId INT
AS
BEGIN
    
    SELECT TaskID, TaskName, TaskCategory, ScheduleType, ScheduleValue, IsActive, ShiftID
    FROM [dbo].[Tasks]
    WHERE TaskID = @TaskId;
END
