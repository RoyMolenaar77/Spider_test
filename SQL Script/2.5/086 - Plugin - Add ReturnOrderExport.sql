  IF NOT exists (select * from plugin where [plugintype] = 'Concentrator.Plugins.PFA.Export.WehkampReturnOrdersExportPFA')
  begin 
      insert into plugin (pluginname, plugintype, plugingroup, cronexpression, executeonstartup, isactive, jobserver)
		  values     ('PFA Wehkamp return orders export', 'Concentrator.Plugins.PFA.Export.WehkampReturnOrdersExportPFA', 'Export', '0 0/5 * * * ?', 0,0, 1)
    print 'PFA Wehkamp return orders export'
  end
else
  begin
    print 'Plugin already added'
  end
