 BEGIN TRAN

  IF EXISTS(SELECT * FROM [sys].[columns] WHERE [object_id] = OBJECT_ID('dbo.ProductGroupVendor') AND [name] = 'ProductGroupID')
  BEGIN
  alter table ProductGroupVendor ALTER COLUMN ProductGroupID INTEGER NULL
	  PRINT 'Altered column ProductGroupID on table productgroupvendor set null'
  END
  ELSE
  BEGIN
    PRINT 'Column [ProductGroupID] does not exist on table ProductGroupVendor'
  END

  DECLARE @ErrorCode INT = @@ERROR

  IF (@ErrorCode <> 0)
    GOTO PROBLEM

COMMIT TRAN

PROBLEM:
  IF (@ErrorCode <> 0)
  BEGIN
	  ROLLBACK TRAN
	  PRINT 'Column [ProductGroupID] could  not be made null on table [dbo].[ProductGroupVendor]' 
  END
