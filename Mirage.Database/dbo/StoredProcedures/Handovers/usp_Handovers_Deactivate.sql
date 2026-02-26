CREATE PROCEDURE [dbo].[usp_Handovers_Deactivate]
    @HandoverId INT,
    @UserId INT,
    @Reason NVARCHAR(MAX)
AS
BEGIN
    SET XACT_ABORT ON;
    UPDATE [dbo].[Handovers] 
    SET IsActive = 0, 
        DeactivationReason = @Reason, 
        DeactivatedByUserID = @UserId, 
        DeactivationDateTime = GETUTCDATE()
    WHERE HandoverID = @HandoverId 
      AND IsActive = 1;
END
