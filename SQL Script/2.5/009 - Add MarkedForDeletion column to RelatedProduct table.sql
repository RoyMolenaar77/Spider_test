IF NOT exists(select * from sys.columns where Name = N'MarkedForDeletion' and Object_ID = Object_ID(N'RelatedProduct'))    
  begin
    ALTER TABLE [dbo].[RelatedProduct]
    ADD [MarkedForDeletion] bit NOT NULL DEFAULT(0)
    print 'MarkedForDeletion Index column'
  end
else
  begin
    print 'Column [MarkedForDeletion] already added to the table'
  end
 
 