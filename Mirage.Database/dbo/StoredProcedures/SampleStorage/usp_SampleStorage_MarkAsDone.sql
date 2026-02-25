CREATE PROCEDURE [dbo].[usp_SampleStorage_MarkAsDone]
    @StorageId INT,
    @UserId INT
AS
BEGIN
        SET XACT_ABORT ON;
    UPDATE [dbo].[SampleStorage] 
    SET IsTestDone = 1, 
        TestDoneByUserID = @UserId, 
        TestDoneDateTime = GETUTCDATE() 
    WHERE StorageID = @StorageId 
      AND IsTestDone = 0;
END
