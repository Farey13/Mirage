CREATE PROCEDURE [dbo].[usp_AdminListItems_GetAll]
AS
BEGIN
    SELECT ItemID, ListType, ItemValue, Description, IsActive
    FROM [dbo].[AdminListItems]
    ORDER BY ListType, ItemValue;
END
