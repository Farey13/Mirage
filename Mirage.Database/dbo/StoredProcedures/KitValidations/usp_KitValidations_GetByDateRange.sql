CREATE PROCEDURE [dbo].[usp_KitValidations_GetByDateRange]
    @StartDate DATE,
    @EndDate DATE
AS
BEGIN
    
    DECLARE @InclusiveEndDate DATE = DATEADD(day, 1, @EndDate);
    SELECT ValidationID, KitName, KitLotNumber, KitExpiryDate, ValidationStatus, Comments, 
           ValidationDateTime, ValidatedByUserID, IsActive, DeactivationReason, DeactivatedByUserID, DeactivationDateTime
    FROM [dbo].[KitValidations]
    WHERE IsActive = 1 
      AND ValidationDateTime >= @StartDate 
      AND ValidationDateTime < @InclusiveEndDate
    ORDER BY ValidationDateTime DESC;
END
