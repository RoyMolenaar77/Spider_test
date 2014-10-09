IF NOT exists (select * from plugin where [plugintype] = 'Concentrator.Plugins.PFA.Transfer.ReceivedTransferProcessor')
  begin 
      insert into plugin (pluginname, plugintype, plugingroup, cronexpression, executeonstartup, isactive, jobserver)
		  values     ('Received Transfer Processor', 'Concentrator.Plugins.PFA.Transfer.ReceivedTransferProcessor', 'Import', '0 0 6 * * ?', 0,0, 2)
    print 'Added plugin received transfer processor'
  end
else
  begin
    print 'Received transfer processor already added'
  end
