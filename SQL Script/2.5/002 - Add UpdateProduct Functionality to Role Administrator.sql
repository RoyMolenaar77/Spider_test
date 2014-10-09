IF NOT exists (select * from functionalityrole where functionalityname = 'UpdateProduct' and roleid = 1)
  begin 
    insert into functionalityrole values (1, 'UpdateProduct')
    print 'Inserted functionality for Administrator'
  end
else
  begin
    print 'Functionality already added'
  end