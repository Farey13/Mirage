CREATE PROCEDURE [dbo].[usp_Shifts_Deactivate]
    @ShiftId INT
AS
BEGIN
    SET XACT_ABORT ON;
    UPDATE [dbo].[Shifts]
    SET IsActive = 0
    WHERE ShiftID = @ShiftId;
END
