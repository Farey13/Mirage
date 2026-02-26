CREATE PROCEDURE [dbo].[usp_KitValidations_Deactivate]
    @ValidationId INT,
    @UserId INT,
    @Reason NVARCHAR(MAX)
AS
BEGIN
    SET XACT_ABORT ON;
    UPDATE [dbo].[KitValidations]
    SET IsActive = 0, 
        DeactivationReason = @Reason, 
        DeactivatedByUserID = @UserId, 
        DeactivationDateTime = GETUTCDATE()
    WHERE ValidationID = @ValidationId 
      AND IsActive = 1;
END
