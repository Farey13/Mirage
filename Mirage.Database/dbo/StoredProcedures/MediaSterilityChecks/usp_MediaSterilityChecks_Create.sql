CREATE PROCEDURE [dbo].[usp_MediaSterilityChecks_Create]
    @MediaName NVARCHAR(255),
    @MediaLotNumber NVARCHAR(100),
    @MediaQuantity NVARCHAR(100),
    @Result37C NVARCHAR(50),
    @Result25C NVARCHAR(50),
    @OverallStatus NVARCHAR(50),
    @Comments NVARCHAR(MAX),
    @PerformedByUserID INT
AS
BEGIN
        SET XACT_ABORT ON;
    INSERT INTO [dbo].[MediaSterilityChecks] (MediaName, MediaLotNumber, MediaQuantity, Result37C, Result25C, OverallStatus, Comments, PerformedByUserID)
    OUTPUT INSERTED.SterilityCheckID, INSERTED.MediaName, INSERTED.MediaLotNumber, INSERTED.MediaQuantity, 
           INSERTED.Result37C, INSERTED.Result25C, INSERTED.OverallStatus, INSERTED.Comments, 
           INSERTED.CheckDateTime, INSERTED.PerformedByUserID, INSERTED.IsActive, 
           INSERTED.DeactivationReason, INSERTED.DeactivatedByUserID, INSERTED.DeactivationDateTime
    VALUES (@MediaName, @MediaLotNumber, @MediaQuantity, @Result37C, @Result25C, @OverallStatus, @Comments, @PerformedByUserID);
END
