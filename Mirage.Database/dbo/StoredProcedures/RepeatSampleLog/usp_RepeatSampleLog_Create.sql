CREATE PROCEDURE [dbo].[usp_RepeatSampleLog_Create]
    @PatientIdCardNumber NVARCHAR(100),
    @PatientName NVARCHAR(255),
    @ReasonText NVARCHAR(MAX),
    @InformedPerson NVARCHAR(255),
    @Department NVARCHAR(50),
    @LoggedByUserID INT
AS
BEGIN
        SET XACT_ABORT ON;
    INSERT INTO [dbo].[RepeatSampleLog] (PatientIdCardNumber, PatientName, ReasonText, InformedPerson, Department, LoggedByUserID)
    OUTPUT INSERTED.RepeatID, INSERTED.PatientIdCardNumber, INSERTED.PatientName, INSERTED.ReasonText, 
           INSERTED.InformedPerson, INSERTED.Department, INSERTED.LogDateTime, INSERTED.LoggedByUserID, 
           INSERTED.IsActive, INSERTED.DeactivationReason, INSERTED.DeactivatedByUserID, INSERTED.DeactivationDateTime
    VALUES (@PatientIdCardNumber, @PatientName, @ReasonText, @InformedPerson, @Department, @LoggedByUserID);
END
