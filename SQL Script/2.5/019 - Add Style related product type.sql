﻿IF NOT exists (select * from relatedproducttype where [type] = 'style')
  begin 
    insert into relatedproducttype values ('Style', 0)
    
    print 'Inserted related product type'
  end
else
  begin
    print 'related product type already added'
  end