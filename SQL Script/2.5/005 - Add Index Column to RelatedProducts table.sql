IF NOT exists(select * from sys.columns where Name = N'Index' and Object_ID = Object_ID(N'RelatedProduct'))    
  begin
    ALTER TABLE [dbo].[RelatedProduct]
    ADD [Index] int NOT NULL DEFAULT(0)
    print 'Added Index column'
  end
else
  begin
    print 'Column [Index] already added to the table'
  end
 
 