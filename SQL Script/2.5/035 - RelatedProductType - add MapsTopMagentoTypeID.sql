
IF NOT exists(select * from sys.columns where Name = N'TypeMapsToMagentoTypeID' and Object_ID = Object_ID(N'relatedproducttype'))    
  begin
   alter table relatedproducttype add TypeMapsToMagentoTypeID int null
    print 'Added TypeMapsToMagentoTypeID column'
  end
else
  begin
    print 'Column TypeMapsToMagentoTypeID already added'
  end

