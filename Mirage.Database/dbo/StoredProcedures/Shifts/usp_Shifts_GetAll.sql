CREATE PROCEDURE [dbo].[usp_Shifts_GetAll]
AS
BEGIN
    
    SELECT ShiftID, ShiftName, StartTime, EndTime, GracePeriodHours, IsActive
    FROM [dbo].[Shifts]
    WHERE IsActive = 1;
END
