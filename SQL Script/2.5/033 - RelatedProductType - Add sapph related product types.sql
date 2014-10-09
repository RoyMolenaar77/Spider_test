IF NOT exists (select * from relatedproducttype where [type] = 'ModelType')
  begin 
    insert into relatedproducttype values ('ModelType', 0)
    
    print 'Inserted related product type'
  end
else
  begin
    print 'related product type already added'
  end

  IF NOT exists (select * from relatedproducttype where [type] = 'ModelTypeCross')
  begin 
    insert into relatedproducttype values ('ModelTypeCross', 0)
    
    print 'Inserted related product type'
  end
else
  begin
    print 'related product type already added'
  end