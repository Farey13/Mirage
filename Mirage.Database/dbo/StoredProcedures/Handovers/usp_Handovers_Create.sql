CREATE PROCEDURE [dbo].[usp_Handovers_Create]
    @HandoverNotes NVARCHAR(MAX),
    @Priority NVARCHAR(50),
    @Shift NVARCHAR(50),
    @GivenByUserID INT
AS
BEGIN
        SET XACT_ABORT ON;
    INSERT INTO [dbo].[Handovers] (HandoverNotes, Priority, Shift, GivenByUserID)
    OUTPUT INSERTED.HandoverID, INSERTED.HandoverNotes, INSERTED.GivenDateTime, INSERTED.GivenByUserID, 
           INSERTED.IsReceived, INSERTED.ReceivedDateTime, INSERTED.ReceivedByUserID, INSERTED.Priority, 
           INSERTED.Shift, INSERTED.IsActive, INSERTED.DeactivationReason, INSERTED.DeactivatedByUserID, 
           INSERTED.DeactivationDateTime
    VALUES (@HandoverNotes, @Priority, @Shift, @GivenByUserID);
END
