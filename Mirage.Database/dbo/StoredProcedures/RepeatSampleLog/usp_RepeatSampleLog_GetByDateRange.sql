CREATE PROCEDURE [dbo].[usp_RepeatSampleLog_GetByDateRange]
    @StartDate DATE,
    @EndDate DATE
AS
BEGIN
    
    DECLARE @InclusiveEndDate DATE = DATEADD(day, 1, @EndDate);
    SELECT RepeatID, PatientIdCardNumber, PatientName, ReasonText, InformedPerson, Department, 
           LogDateTime, LoggedByUserID, IsActive, DeactivationReason, DeactivatedByUserID, DeactivationDateTime
    FROM [dbo].[RepeatSampleLog]
    WHERE IsActive = 1 
      AND LogDateTime >= @StartDate 
      AND LogDateTime < @InclusiveEndDate
    ORDER BY LogDateTime DESC;
END
