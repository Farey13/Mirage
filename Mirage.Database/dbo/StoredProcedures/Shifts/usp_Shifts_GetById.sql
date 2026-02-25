CREATE PROCEDURE [dbo].[usp_Shifts_GetById]
    @ShiftId INT
AS
BEGIN
    
    SELECT ShiftID, ShiftName, StartTime, EndTime, GracePeriodHours, IsActive
    FROM [dbo].[Shifts]
    WHERE ShiftID = @ShiftId;
END
