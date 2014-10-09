DECLARE @Length INT = 250

IF (SELECT TOP 1 [max_length] FROM [sys].[columns] WHERE [object_id] = OBJECT_ID('[dbo].[ManagementLabel]') AND [name] = 'Field') < @Length
  ALTER TABLE [dbo].[ManagementLabel] ALTER COLUMN [Field] NVARCHAR(250) NOT NULL

IF (SELECT TOP 1 [max_length] FROM [sys].[columns] WHERE [object_id] = OBJECT_ID('[dbo].[ManagementLabel]') AND [name] = 'Label') < @Length
  ALTER TABLE [dbo].[ManagementLabel] ALTER COLUMN [Label] NVARCHAR(250) NOT NULL

IF NOT EXISTS(SELECT * FROM [sys].[columns] WHERE [object_id] = OBJECT_ID('[dbo].[ManagementLabel]') AND [name] = 'Grid')
  ALTER TABLE [dbo].[ManagementLabel] ADD [Grid] NVARCHAR(250) NOT NULL DEFAULT ''

IF NOT EXISTS(SELECT * FROM [sys].[columns] WHERE [object_id] = OBJECT_ID('[dbo].[ManagementLabel]') AND [name] = 'UserID')
  ALTER TABLE [dbo].[ManagementLabel] ADD [UserID] INT NOT NULL DEFAULT 1