USE MirageDB;
GO

INSERT INTO Tasks (TaskName, TaskCategory, ScheduleType, ScheduleValue, IsActive)
VALUES
('Check Refrigerator Temperatures', 'Morning', 'Daily', NULL, 1),
('Perform Weekly Maintenance on Centrifuge', 'Morning', 'Weekly', 'Sunday', 1),
('Monthly Deep Clean of Glassware', 'Evening', 'Monthly', '21', 1);
GO