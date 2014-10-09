IF NOT exists (select * from plugin where [plugintype] = 'Concentrator.Plugins.Pfa.Imports.Stock.PFAStockImportCC')
  begin 
      insert into plugin (pluginname, plugintype, plugingroup, cronexpression, executeonstartup, isactive, jobserver)
		  values     ('PFA Stock Import CC', 'Concentrator.Plugins.PFA.Imports.Stock.PFAStockImportCC', 'Import', '0 0 6 * * ?', 0,0, 2)
    print 'Inserted PFA Stock import CC plugin'
  end
else
  begin
    print 'Plugin already added'
  end


  IF NOT exists (select * from plugin where [plugintype] = 'Concentrator.Plugins.Pfa.Imports.Stock.PFAStockImportAT')
  begin 
      insert into plugin (pluginname, plugintype, plugingroup, cronexpression, executeonstartup, isactive, jobserver)
		  values     ('PFA Stock Import AT', 'Concentrator.Plugins.PFA.Imports.Stock.PFAStockImportAT', 'Import', '0 0 6 * * ?', 0,0,3)
    print 'Inserted PFA Stock import AT plugin'
  end
else
  begin
    print 'Plugin already added'
  end

update plugin set cronexpression = '0 0 3 * * ?' where plugintype = 'Concentrator.Plugins.PFA.PFAImport'

update plugin set cronexpression = '0 15 3 * * ?' where plugintype = 'Concentrator.Plugins.PFA.ATPFAImport'
