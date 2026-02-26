CREATE PROCEDURE [dbo].[usp_MachineBreakdowns_Deactivate]
    @BreakdownId INT,
    @UserId INT,
    @Reason NVARCHAR(MAX)
AS
BEGIN
    SET XACT_ABORT ON;
    UPDATE [dbo].[MachineBreakdowns] 
    SET IsActive = 0, 
        DeactivationReason = @Reason, 
        DeactivatedByUserID = @UserId, 
        DeactivationDateTime = GETUTCDATE()
    WHERE BreakdownID = @BreakdownId 
      AND IsActive = 1;
END
