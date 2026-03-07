CREATE PROCEDURE [dbo].[usp_Tasks_GetAll]
    @IsDeleted BIT = 0
AS
BEGIN
    
    SELECT TaskID, TaskName, TaskCategory, ScheduleType, ScheduleValue, IsActive, ShiftID, IsDeleted
    FROM [dbo].[Tasks]
    WHERE IsActive = 1
      AND IsDeleted = @IsDeleted;
END
