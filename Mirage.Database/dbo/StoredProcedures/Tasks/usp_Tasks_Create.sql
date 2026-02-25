CREATE PROCEDURE [dbo].[usp_Tasks_Create]
    @TaskName NVARCHAR(255),
    @ShiftID INT,
    @ScheduleType NVARCHAR(50),
    @ScheduleValue NVARCHAR(50),
    @IsActive BIT
AS
BEGIN
        SET XACT_ABORT ON;
    INSERT INTO [dbo].[Tasks] (TaskName, ShiftID, ScheduleType, ScheduleValue, IsActive)
    OUTPUT INSERTED.TaskID, INSERTED.TaskName, INSERTED.TaskCategory, INSERTED.ScheduleType, INSERTED.ScheduleValue, INSERTED.IsActive, INSERTED.ShiftID
    VALUES (@TaskName, @ShiftID, @ScheduleType, @ScheduleValue, @IsActive);
END
