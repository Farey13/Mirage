CREATE PROCEDURE [dbo].[usp_Shifts_Create]
    @ShiftName NVARCHAR(100),
    @StartTime TIME(7),
    @EndTime TIME(7),
    @GracePeriodHours INT,
    @IsActive BIT
AS
BEGIN
        SET XACT_ABORT ON;
    INSERT INTO [dbo].[Shifts] (ShiftName, StartTime, EndTime, GracePeriodHours, IsActive)
    OUTPUT INSERTED.ShiftID, INSERTED.ShiftName, INSERTED.StartTime, INSERTED.EndTime, INSERTED.GracePeriodHours, INSERTED.IsActive
    VALUES (@ShiftName, @StartTime, @EndTime, @GracePeriodHours, @IsActive);
END
