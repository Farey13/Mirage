CREATE PROCEDURE [dbo].[usp_AdminListItems_Update]
    @ItemID INT,
    @ItemValue NVARCHAR(255),
    @Description NVARCHAR(500),
    @IsActive BIT
AS
BEGIN
        SET XACT_ABORT ON;
    UPDATE [dbo].[AdminListItems]
    SET ItemValue = @ItemValue,
        Description = @Description,
        IsActive = @IsActive
    OUTPUT INSERTED.ItemID, INSERTED.ListType, INSERTED.ItemValue, INSERTED.Description, INSERTED.IsActive
    WHERE ItemID = @ItemID;
END
