USE MirageDB;
GO

-- Insert the setting only if it doesn't already exist
IF NOT EXISTS (SELECT 1 FROM AdminListItems WHERE ListType = 'SystemSetting' AND ItemValue = 'InactivityTimeoutMinutes')
BEGIN
    INSERT INTO AdminListItems (ListType, ItemValue, Description, IsActive)
    VALUES ('SystemSetting', 'InactivityTimeoutMinutes', '15', 1); -- The '15' in Description is the timeout in minutes
END
GO