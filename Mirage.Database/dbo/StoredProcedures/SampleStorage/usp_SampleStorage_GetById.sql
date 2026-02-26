CREATE PROCEDURE [dbo].[usp_SampleStorage_GetById]
    @StorageId INT
AS
BEGIN
    
    SELECT StorageID, PatientSampleID, StorageDateTime, StoredByUserID, IsTestDone, 
           TestDoneDateTime, TestDoneByUserID, TestName, IsActive, 
           DeactivationReason, DeactivatedByUserID, DeactivationDateTime
    FROM [dbo].[SampleStorage] 
    WHERE StorageID = @StorageId;
END
