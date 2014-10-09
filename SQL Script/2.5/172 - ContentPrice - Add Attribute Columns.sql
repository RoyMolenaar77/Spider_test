BEGIN TRAN

  IF NOT EXISTS(SELECT * FROM [sys].[columns] WHERE [object_id] = OBJECT_ID('dbo.ContentPrice') AND [name] = 'AttributeID')
  BEGIN
    alter table contentprice 
    add AttributeID int null

    alter table contentprice
    add AttributeValue nvarchar(1000) null

alter table contentprice add constraint FK_ContentPrice_Attribute foreign key (AttributeID) references ProductAttributeMetaData(AttributeID)
  END
  ELSE
  BEGIN
    PRINT 'Column [ContentPrice] already added to table [dbo].[AttributeID]'
  END

  DECLARE @ErrorCode INT = @@ERROR

  IF (@ErrorCode <> 0)
    GOTO PROBLEM

COMMIT TRAN

PROBLEM:
  IF (@ErrorCode <> 0)
  BEGIN
	  ROLLBACK TRAN
	  PRINT 'Column [AttributeID] could not be added to table [dbo].[ContentPrice]' 
  END