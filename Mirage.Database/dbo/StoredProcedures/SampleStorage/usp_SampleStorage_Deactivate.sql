CREATE PROCEDURE [dbo].[usp_SampleStorage_Deactivate]
    @StorageId INT,
    @UserId INT,
    @Reason NVARCHAR(MAX)
AS
BEGIN
    SET XACT_ABORT ON;
    UPDATE [dbo].[SampleStorage] 
    SET IsActive = 0, 
        DeactivationReason = @Reason,
        DeactivatedByUserID = @UserId,
        DeactivationDateTime = GETUTCDATE()
    WHERE StorageID = @StorageId 
      AND IsActive = 1;
END
