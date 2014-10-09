DECLARE @intErrorCode INT

BEGIN TRAN
	--Begin Action
  IF NOT exists (select * from plugin where plugintype = 'Concentrator.Plugins.ConnectorProductSync.AssortmentGeneratorV2')
    begin 
      insert into Plugin (PluginName, PluginType, PluginGroup, CronExpression, ExecuteOnStartup, IsActive, JobServer)  
      values ('AssortmentGeneratorV2', 'Concentrator.Plugins.ConnectorProductSync.AssortmentGeneratorV2', 'Internal', '0 0 0/1 * * ?', 0, 0, 1)
      print 'Inserted GenerateAssortment V2 plugin'
    end
  else
    begin
      print 'GenerateAssortment V2 plugin already added'
    end
	--End Action 

	SELECT @intErrorCode = @@ERROR
    IF (@intErrorCode <> 0) GOTO PROBLEM
COMMIT TRAN

PROBLEM:
  IF (@intErrorCode <> 0) 
  BEGIN
    PRINT 'Unexpected error occurred while <add description>!'
    ROLLBACK TRAN
END

