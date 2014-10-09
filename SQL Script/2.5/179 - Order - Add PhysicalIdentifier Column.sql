BEGIN TRAN

  IF NOT EXISTS(SELECT * FROM [sys].[columns] WHERE [object_id] = OBJECT_ID('dbo.[Order]') AND [name] = 'PhysicalIdentifier')
  BEGIN
    alter table [Order]
    add PhysicalIdentifier nvarchar(100) null

  END
  ELSE
  BEGIN
    PRINT 'Column [PhysicalIdentifier] already added to table [dbo].[Order]'
  END

  DECLARE @ErrorCode INT = @@ERROR

  IF (@ErrorCode <> 0)
    GOTO PROBLEM

COMMIT TRAN

PROBLEM:
  IF (@ErrorCode <> 0)
  BEGIN
	  ROLLBACK TRAN
	  PRINT 'Column [PhysicalIdentifier] could not be added to table [dbo].[Order]' 
  END