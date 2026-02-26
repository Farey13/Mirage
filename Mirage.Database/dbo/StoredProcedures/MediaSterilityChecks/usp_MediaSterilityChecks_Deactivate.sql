CREATE PROCEDURE [dbo].[usp_MediaSterilityChecks_Deactivate]
    @CheckId INT,
    @UserId INT,
    @Reason NVARCHAR(MAX)
AS
BEGIN
    SET XACT_ABORT ON;
    UPDATE [dbo].[MediaSterilityChecks]
    SET IsActive = 0, 
        DeactivationReason = @Reason, 
        DeactivatedByUserID = @UserId, 
        DeactivationDateTime = GETUTCDATE()
    WHERE SterilityCheckID = @CheckId 
      AND IsActive = 1;
END
