CREATE PROCEDURE [dbo].[usp_AdminListItems_GetByType]
    @ListType NVARCHAR(100)
AS
BEGIN
    
    SELECT ItemID, ListType, ItemValue, Description, IsActive
    FROM [dbo].[AdminListItems]
    WHERE ListType = @ListType
    ORDER BY ItemValue;
END
