  IF NOT exists (select * from plugin where [plugintype] = 'Concentrator.Plugins.PfaCommunicator.Communicator')
  begin 
      insert into plugin (pluginname, plugintype, plugingroup, cronexpression, executeonstartup, isactive, jobserver)
		  values     ('PFA Communicator', 'Concentrator.Plugins.PfaCommunicator.Communicator', 'Import', '0 0/5 * * * ?', 0,0, 1)
    print 'Inserted PFA Communicator'
  end
else
  begin
    print 'Plugin already added'
  end
