IF NOT exists(select * from sys.columns where Name = N'IsExported' and Object_ID = Object_ID(N'ContentProductGroup'))    
  begin
    ALTER TABLE [dbo].ContentProductGroup
    ADD IsExported bit NOT NULL DEFAULT(1)
    print 'Added IsExported column'
  end
else
  begin
    print 'Column already added to the table'
  end
 
 