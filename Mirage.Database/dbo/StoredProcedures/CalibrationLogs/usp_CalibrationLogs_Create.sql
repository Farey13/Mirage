CREATE PROCEDURE [dbo].[usp_CalibrationLogs_Create]
    @TestName NVARCHAR(255),
    @QcResult NVARCHAR(50),
    @Reason NVARCHAR(MAX),
    @PerformedByUserID INT
AS
BEGIN
        SET XACT_ABORT ON;
    INSERT INTO [dbo].[CalibrationLogs] (TestName, QcResult, Reason, PerformedByUserID)
    OUTPUT INSERTED.CalibrationID, INSERTED.TestName, INSERTED.QcResult, INSERTED.Reason, 
           INSERTED.CalibrationDateTime, INSERTED.PerformedByUserID, INSERTED.IsActive, 
           INSERTED.DeactivationReason, INSERTED.DeactivatedByUserID, INSERTED.DeactivationDateTime
    VALUES (@TestName, @QcResult, @Reason, @PerformedByUserID);
END
