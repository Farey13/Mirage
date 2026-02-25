CREATE PROCEDURE [dbo].[usp_SampleStorage_GetCompletedByDateRange]
    @StartDate DATE,
    @EndDate DATE
AS
BEGIN
    
    DECLARE @InclusiveEndDate DATE = DATEADD(day, 1, @EndDate);
    SELECT StorageID, PatientSampleID, StorageDateTime, StoredByUserID, IsTestDone, 
           TestDoneDateTime, TestDoneByUserID, TestName, IsActive, 
           DeactivationReason, DeactivatedByUserID, DeactivationDateTime
    FROM [dbo].[SampleStorage] 
    WHERE IsTestDone = 1 
      AND IsActive = 1
      AND StorageDateTime >= @StartDate 
      AND StorageDateTime < @InclusiveEndDate
    ORDER BY StorageDateTime DESC;
END
