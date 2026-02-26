CREATE PROCEDURE [dbo].[usp_SampleStorage_GetReportData]
    @StartDate DATE,
    @EndDate DATE,
    @TestName NVARCHAR(255) = NULL,
    @Status NVARCHAR(50) = NULL
AS
BEGIN
    
    DECLARE @InclusiveEndDate DATE = DATEADD(day, 1, @EndDate);
    SELECT 
        ss.StorageDateTime,
        ss.PatientSampleID,
        ss.TestName,
        u_stored.FullName AS StoredByUsername,
        ss.IsTestDone,
        ss.TestDoneDateTime,
        u_done.FullName AS TestDoneByUsername
    FROM [dbo].[SampleStorage] ss
    LEFT JOIN [dbo].[Users] u_stored ON ss.StoredByUserID = u_stored.UserID
    LEFT JOIN [dbo].[Users] u_done ON ss.TestDoneByUserID = u_done.UserID
    WHERE ss.IsActive = 1 
      AND ss.StorageDateTime >= @StartDate 
      AND ss.StorageDateTime < @InclusiveEndDate
      AND (@TestName IS NULL OR @TestName = 'All' OR ss.TestName = @TestName)
      AND (@Status IS NULL 
           OR (@Status = 'Pending' AND ss.IsTestDone = 0) 
           OR (@Status = 'Test Done' AND ss.IsTestDone = 1))
    ORDER BY ss.StorageDateTime DESC;
END
