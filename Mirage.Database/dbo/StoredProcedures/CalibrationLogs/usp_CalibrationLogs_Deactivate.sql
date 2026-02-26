CREATE PROCEDURE [dbo].[usp_CalibrationLogs_Deactivate]
    @LogId INT,
    @UserId INT,
    @Reason NVARCHAR(MAX)
AS
BEGIN
    SET XACT_ABORT ON;
    UPDATE [dbo].[CalibrationLogs]
    SET IsActive = 0, 
        DeactivationReason = @Reason, 
        DeactivatedByUserID = @UserId, 
        DeactivationDateTime = GETUTCDATE()
    WHERE CalibrationID = @LogId 
      AND IsActive = 1;
END
