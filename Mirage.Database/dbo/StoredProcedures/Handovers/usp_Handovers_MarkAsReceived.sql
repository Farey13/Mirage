CREATE PROCEDURE [dbo].[usp_Handovers_MarkAsReceived]
    @HandoverId INT,
    @UserId INT
AS
BEGIN
        SET XACT_ABORT ON;
    UPDATE [dbo].[Handovers] 
    SET IsReceived = 1, 
        ReceivedByUserID = @UserId, 
        ReceivedDateTime = GETUTCDATE() 
    WHERE HandoverID = @HandoverId 
      AND IsReceived = 0;
END
