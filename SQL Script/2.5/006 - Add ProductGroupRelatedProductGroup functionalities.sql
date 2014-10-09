IF NOT exists (select * from functionalityrole where functionalityname = 'GetProductGroupRelatedProductGroup' and roleid = 1)
  begin 
    insert into functionalityrole values (1, 'GetProductGroupRelatedProductGroup')
    print 'Inserted GetProductGroupRelatedProductGroup functionality for Administrator'
  end
else
  begin
    print 'Functionality GetProductGroupRelatedProductGroup already added'
  end


  IF NOT exists (select * from functionalityrole where functionalityname = 'UpdateProductGroupRelatedProductGroup' and roleid = 1)
  begin 
    insert into functionalityrole values (1, 'UpdateProductGroupRelatedProductGroup')
    print 'Inserted UpdateProductGroupRelatedProductGroup functionality for Administrator'
  end
else
  begin
    print 'Functionality UpdateProductGroupRelatedProductGroup already added'
  end



  IF NOT exists (select * from functionalityrole where functionalityname = 'DeleteProductGroupRelatedProductGroup' and roleid = 1)
  begin 
    insert into functionalityrole values (1, 'DeleteProductGroupRelatedProductGroup')
    print 'Inserted DeleteProductGroupRelatedProductGroup functionality for Administrator'
  end
else
  begin
    print 'Functionality DeleteProductGroupRelatedProductGroup already added'
  end



  IF NOT exists (select * from functionalityrole where functionalityname = 'CreateProductGroupRelatedProductGroup' and roleid = 1)
  begin 
    insert into functionalityrole values (1, 'CreateProductGroupRelatedProductGroup')
    print 'Inserted CreateProductGroupRelatedProductGroup functionality for Administrator'
  end
else
  begin
    print 'Functionality CreateProductGroupRelatedProductGroup already added'
  end