DECLARE @intErrorCode INT

BEGIN TRAN

  --Begin Action
	IF NOT EXISTS ( SELECT * FROM sys.tables WHERE NAME = '[RefundQueueHistory]' )
		
		BEGIN	

	    CREATE TABLE [dbo].[RefundQueueHistory](
			OrderID [int] NOT NULL,
			OrderResponseID [int] NOT NULL,
			CreationTime DateTime not null,
      Amount decimal(18,2) not null,
			ProcessedTime DateTime not null default((getdate()))
		
     CONSTRAINT [PK_RefundQueueHistory] PRIMARY KEY CLUSTERED 
    (
	    OrderID ASC,
	    OrderResponseID ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
    ) ON [PRIMARY]
     
    END
  ELSE
    BEGIN
      PRINT 'Table RefundQueue already added to the database'
    END
  --End Action 

  SELECT @intErrorCode = @@ERROR
  IF (@intErrorCode <> 0)
    GOTO PROBLEM

COMMIT TRAN
PROBLEM:

IF (@intErrorCode <> 0)
BEGIN
  PRINT 'Unexpected error occurred while creating table RefundQueue'

  ROLLBACK TRAN
END