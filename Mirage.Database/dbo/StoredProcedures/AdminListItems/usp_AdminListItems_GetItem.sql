CREATE PROCEDURE [dbo].[usp_AdminListItems_GetItem]
    @ListType NVARCHAR(100),
    @ItemValue NVARCHAR(255)
AS
BEGIN
    
    SELECT ItemID, ListType, ItemValue, Description, IsActive
    FROM [dbo].[AdminListItems]
    WHERE ListType = @ListType 
      AND ItemValue = @ItemValue;
END
