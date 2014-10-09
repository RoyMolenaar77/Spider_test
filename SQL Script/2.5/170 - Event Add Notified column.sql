BEGIN TRAN

  IF NOT EXISTS(SELECT * FROM [sys].[columns] WHERE [object_id] = OBJECT_ID('dbo.[Event]') AND [name] = 'Notified')
  BEGIN
    ALTER TABLE [dbo].[Event] ADD [Notified] bit not null default(0)
	  PRINT 'Added column [Notified] to [dbo].[Event]'
  END
  ELSE
  BEGIN
    PRINT 'Column [Notified] already added to table [dbo].[Event]'
  END

  DECLARE @ErrorCode INT = @@ERROR

  IF (@ErrorCode <> 0)
    GOTO PROBLEM

COMMIT TRAN

PROBLEM:
  IF (@ErrorCode <> 0)
  BEGIN
	  ROLLBACK TRAN
	  PRINT 'Column [Notified] could not be added to table [dbo].[Event]' 
  END