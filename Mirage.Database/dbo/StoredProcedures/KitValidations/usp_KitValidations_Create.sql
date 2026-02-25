CREATE PROCEDURE [dbo].[usp_KitValidations_Create]
    @KitName NVARCHAR(255),
    @KitLotNumber NVARCHAR(100),
    @KitExpiryDate DATE,
    @ValidationStatus NVARCHAR(50),
    @Comments NVARCHAR(MAX),
    @ValidatedByUserID INT
AS
BEGIN
        SET XACT_ABORT ON;
    INSERT INTO [dbo].[KitValidations] (KitName, KitLotNumber, KitExpiryDate, ValidationStatus, Comments, ValidatedByUserID)
    OUTPUT INSERTED.ValidationID, INSERTED.KitName, INSERTED.KitLotNumber, INSERTED.KitExpiryDate, 
           INSERTED.ValidationStatus, INSERTED.Comments, INSERTED.ValidationDateTime, INSERTED.ValidatedByUserID, 
           INSERTED.IsActive, INSERTED.DeactivationReason, INSERTED.DeactivatedByUserID, INSERTED.DeactivationDateTime
    VALUES (@KitName, @KitLotNumber, @KitExpiryDate, @ValidationStatus, @Comments, @ValidatedByUserID);
END
