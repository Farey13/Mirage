CREATE PROCEDURE [dbo].[usp_Handovers_GetById]
    @HandoverId INT
AS
BEGIN
    
    SELECT HandoverID, HandoverNotes, GivenDateTime, GivenByUserID, IsReceived, 
           ReceivedDateTime, ReceivedByUserID, Priority, Shift, IsActive, 
           DeactivationReason, DeactivatedByUserID, DeactivationDateTime
    FROM [dbo].[Handovers] 
    WHERE HandoverID = @HandoverId;
END
