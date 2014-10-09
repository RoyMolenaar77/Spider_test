IF NOT exists (select * from plugin where [plugintype] = 'Concentrator.Plugins.PFA.Transfer.WehkampTransferOrders')
  begin 
      insert into plugin (pluginname, plugintype, plugingroup, cronexpression, executeonstartup, isactive, jobserver)
                values     ('Wehkamp transfer orders', 'Concentrator.Plugins.PFA.Transfer.WehkampTransferOrders', 'Import', '0 0 6 * * ?', 0,0, 1)
    print 'Inserted Whekamp transfer order'
  end
else
  begin
    print 'Plugin already added'
  end