CREATE PROCEDURE [dbo].[usp_Users_GetByUsername]
    @Username NVARCHAR(100)
AS
BEGIN
    SELECT UserID, Username, PasswordHash, FullName, IsActive, CreatedAt
    FROM [dbo].[Users]
    WHERE Username = @Username;
END
