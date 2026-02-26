CREATE PROCEDURE [dbo].[usp_SampleStorage_Create]
    @PatientSampleID NVARCHAR(100),
    @TestName NVARCHAR(255),
    @StoredByUserID INT
AS
BEGIN
        SET XACT_ABORT ON;
    INSERT INTO [dbo].[SampleStorage] (PatientSampleID, TestName, StoredByUserID)
    OUTPUT INSERTED.StorageID, INSERTED.PatientSampleID, INSERTED.StorageDateTime, INSERTED.StoredByUserID, 
           INSERTED.IsTestDone, INSERTED.TestDoneDateTime, INSERTED.TestDoneByUserID, INSERTED.TestName, 
           INSERTED.IsActive, INSERTED.DeactivationReason, INSERTED.DeactivatedByUserID, INSERTED.DeactivationDateTime
    VALUES (@PatientSampleID, @TestName, @StoredByUserID);
END
