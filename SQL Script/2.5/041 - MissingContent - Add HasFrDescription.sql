IF NOT exists(select * from sys.columns where Name = N'HasFrDescription' and Object_ID = Object_ID(N'MissingContent'))    
  begin
    ALTER TABLE [dbo].[MissingContent]
    ADD [HasFrDescription] bit NOT NULL DEFAULT(0)
    print 'HasFrDescription Index column'
  end
else
  begin
    print 'Column [HasFrDescription] already added to the table'
  end
 
 