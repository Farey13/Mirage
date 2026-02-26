CREATE PROCEDURE [dbo].[usp_Tasks_Deactivate]
    @TaskId INT
AS
BEGIN
    SET XACT_ABORT ON;
    UPDATE [dbo].[Tasks]
    SET IsActive = 0
    WHERE TaskID = @TaskId;
END
