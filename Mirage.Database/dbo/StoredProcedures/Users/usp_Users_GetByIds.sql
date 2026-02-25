CREATE PROCEDURE [dbo].[usp_Users_GetByIds]
    @UserIds NVARCHAR(MAX)
AS
BEGIN
    
    SELECT UserID, Username, PasswordHash, FullName, IsActive, CreatedAt
    FROM [dbo].[Users]
    WHERE UserID IN (SELECT CAST(value AS INT) FROM STRING_SPLIT(@UserIds, ','));
END
