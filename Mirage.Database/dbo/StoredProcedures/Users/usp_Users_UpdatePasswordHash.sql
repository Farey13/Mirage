CREATE PROCEDURE [dbo].[usp_Users_UpdatePasswordHash]
    @UserId INT,
    @NewPasswordHash NVARCHAR(255)
AS
BEGIN
        SET XACT_ABORT ON;
    UPDATE [dbo].[Users]
    SET PasswordHash = @NewPasswordHash
    WHERE UserID = @UserId;
END
