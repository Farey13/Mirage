CREATE PROCEDURE [dbo].[usp_AdminListItems_Create]
    @ListType NVARCHAR(100),
    @ItemValue NVARCHAR(255),
    @Description NVARCHAR(500),
    @IsActive BIT
AS
BEGIN
    SET XACT_ABORT ON;
    INSERT INTO [dbo].[AdminListItems] (ListType, ItemValue, Description, IsActive)
    OUTPUT INSERTED.ItemID, INSERTED.ListType, INSERTED.ItemValue, INSERTED.Description, INSERTED.IsActive
    VALUES (@ListType, @ItemValue, @Description, @IsActive);
END
