CREATE PROCEDURE [dbo].[usp_Users_Create]
    @Username NVARCHAR(100),
    @PasswordHash NVARCHAR(255),
    @FullName NVARCHAR(255),
    @IsActive BIT
AS
BEGIN
    SET XACT_ABORT ON;
    INSERT INTO [dbo].[Users] (Username, PasswordHash, FullName, IsActive)
    OUTPUT INSERTED.UserID, INSERTED.Username, INSERTED.PasswordHash, INSERTED.FullName, INSERTED.IsActive, INSERTED.CreatedAt
    VALUES (@Username, @PasswordHash, @FullName, @IsActive);
END
