IF NOT exists(select * from sys.columns where Name = N'ParentProductID' and Object_ID = Object_ID(N'Product'))    
  begin 
    alter table product add ParentProductID int null 
    alter table product add constraint Product_ParentProduct foreign key (ParentProductID) references Product (ProductID)
    print 'Added ParentProductID column to table Product'
  end
else
  begin
    print 'Column already added to table'
  end