IF NOT exists(select * from sys.columns where Name = N'Visible' and Object_ID = Object_ID(N'Product'))    
  begin
    ALTER TABLE [dbo].[Product]
    ADD Visible bit NOT NULL DEFAULT(1)
    print 'Added Visible column'
  end
else
  begin
    print 'Column already added to the table'
  end
 
 