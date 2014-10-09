
IF NOT EXISTS(SELECT * FROM [sys].[columns] WHERE [object_id] = OBJECT_ID('[dbo].[productattributemetadata]') AND [name] = 'FrontendType')
BEGIN
	alter table productattributemetadata add FrontendType nvarchar(1000) null;
end

