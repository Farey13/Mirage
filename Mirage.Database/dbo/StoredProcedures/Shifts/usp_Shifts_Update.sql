CREATE PROCEDURE [dbo].[usp_Shifts_Update]
    @ShiftID INT,
    @ShiftName NVARCHAR(100),
    @StartTime TIME(7),
    @EndTime TIME(7),
    @GracePeriodHours INT,
    @IsActive BIT
AS
BEGIN
        SET XACT_ABORT ON;
    UPDATE [dbo].[Shifts]
    SET ShiftName = @ShiftName,
        StartTime = @StartTime,
        EndTime = @EndTime,
        GracePeriodHours = @GracePeriodHours,
        IsActive = @IsActive
    WHERE ShiftID = @ShiftID;
END
