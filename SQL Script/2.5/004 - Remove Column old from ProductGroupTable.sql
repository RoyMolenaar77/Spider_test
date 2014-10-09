IF exists(select * from sys.columns where Name = N'old' and Object_ID = Object_ID(N'ProductGroup'))    
  begin 
    alter table productgroup drop column old
    print 'Remove column old from ProductGroup'
  end
else
  begin
    print 'Column already removed from table'
  end