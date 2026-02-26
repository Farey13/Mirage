CREATE PROCEDURE [dbo].[usp_KitValidations_GetReportData]
    @StartDate DATE,
    @EndDate DATE,
    @KitName NVARCHAR(255) = NULL,
    @Status NVARCHAR(50) = NULL
AS
BEGIN
    
    DECLARE @InclusiveEndDate DATE = DATEADD(day, 1, @EndDate);
    SELECT 
        kv.ValidationDateTime,
        kv.KitName,
        kv.KitLotNumber,
        kv.KitExpiryDate,
        kv.ValidationStatus,
        kv.Comments,
        u.FullName AS ValidatedByUsername
    FROM [dbo].[KitValidations] kv
    LEFT JOIN [dbo].[Users] u ON kv.ValidatedByUserID = u.UserID
    WHERE kv.IsActive = 1 
      AND kv.ValidationDateTime >= @StartDate 
      AND kv.ValidationDateTime < @InclusiveEndDate
      AND (@KitName IS NULL OR @KitName = 'All' OR kv.KitName = @KitName)
      AND (@Status IS NULL OR @Status = 'All' OR kv.ValidationStatus = @Status)
    ORDER BY kv.ValidationDateTime DESC;
END
