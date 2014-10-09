IF NOT exists(select * from sys.columns where Name = N'IsActive' and Object_ID = Object_ID(N'RelatedProduct'))    
  begin
    ALTER TABLE [dbo].[RelatedProduct]
    ADD IsActive bit NOT NULL DEFAULT(1)
    print 'Added IsActive column'
  end
else
  begin
    print 'Column already added to the table'
  end
 
 