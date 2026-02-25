CREATE PROCEDURE [dbo].[usp_Users_GetById]
    @UserId INT
AS
BEGIN
    
    SELECT UserID, Username, PasswordHash, FullName, IsActive, CreatedAt
    FROM [dbo].[Users]
    WHERE UserID = @UserId;
END
