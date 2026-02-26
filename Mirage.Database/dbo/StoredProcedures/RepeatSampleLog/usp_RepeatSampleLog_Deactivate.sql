CREATE PROCEDURE [dbo].[usp_RepeatSampleLog_Deactivate]
    @RepeatId INT,
    @UserId INT,
    @Reason NVARCHAR(MAX)
AS
BEGIN
    SET XACT_ABORT ON;
    UPDATE [dbo].[RepeatSampleLog]
    SET IsActive = 0, 
        DeactivationReason = @Reason, 
        DeactivatedByUserID = @UserId, 
        DeactivationDateTime = GETUTCDATE()
    WHERE RepeatID = @RepeatId 
      AND IsActive = 1;
END
