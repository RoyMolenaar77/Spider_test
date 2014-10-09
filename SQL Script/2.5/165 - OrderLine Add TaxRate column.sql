BEGIN TRAN

  IF NOT EXISTS(SELECT * FROM [sys].[columns] WHERE [object_id] = OBJECT_ID('dbo.OrderLine') AND [name] = 'TaxRate')
  BEGIN
    ALTER TABLE [dbo].[OrderLine] ADD [TaxRate] DECIMAL(19,5) NULL
	  PRINT 'Added column [TaxRate] to [dbo].[OrderLine]'
  END
  ELSE
  BEGIN
    PRINT 'Column [TaxRate] already added to table [dbo].[OrderLine]'
  END

  DECLARE @ErrorCode INT = @@ERROR

  IF (@ErrorCode <> 0)
    GOTO PROBLEM

COMMIT TRAN

PROBLEM:
  IF (@ErrorCode <> 0)
  BEGIN
	  ROLLBACK TRAN
	  PRINT 'Column [TaxRate] could not be added to table [dbo].[OrderLine]' 
  END