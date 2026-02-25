CREATE PROCEDURE [dbo].[usp_Users_GetAll]
AS
BEGIN
    
    SELECT UserID, Username, FullName, IsActive, CreatedAt
    FROM [dbo].[Users];
END
