IF NOT exists(select * from sys.columns where Name = N'IsConfigurable' and Object_ID = Object_ID(N'MissingContent'))    
  begin
   alter table missingcontent add
	IsConfigurable bit not null default ((0)),
	HasDescription bit not null default ((0)),
	QuantityOnHand int not null default ((0))
    print 'Added IsConfigurable column'
  end
else
  begin
    print 'Column IsConfigurable already added'
  end


