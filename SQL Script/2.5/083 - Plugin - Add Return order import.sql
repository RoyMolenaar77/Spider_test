  IF NOT exists (select * from plugin where [plugintype] = 'Concentrator.Plugins.PFA.Imports.WehkampReturnOrders.WehkampReturnOrders')
  begin 
      insert into plugin (pluginname, plugintype, plugingroup, cronexpression, executeonstartup, isactive, jobserver)
		  values     ('Excel Return order import', 'Concentrator.Plugins.PFA.Imports.WehkampReturnOrders.WehkampReturnOrders', 'Import', '0 0/5 * * * ?', 0,0, 1)
    print 'Inserted Excel return order import'
  end
else
  begin
    print 'Plugin already added'
  end
