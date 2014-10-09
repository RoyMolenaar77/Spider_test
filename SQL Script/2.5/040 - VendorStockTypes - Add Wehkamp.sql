IF NOT exists (select * from vendorstocktypes where [stocktype] = 'Wehkamp')
  begin 
    insert into VendorStockTypes values ('Wehkamp')
    
    print 'Inserted Wehkamp stock location'
  end
else
  begin
    print 'Stock location already added'
  end