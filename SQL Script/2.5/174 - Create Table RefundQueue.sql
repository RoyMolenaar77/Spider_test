DECLARE @intErrorCode INT

BEGIN TRAN

  --Begin Action
	IF NOT EXISTS ( SELECT * FROM sys.tables WHERE NAME = 'RefundQueue' )
		
		BEGIN	

	    CREATE TABLE [dbo].[RefundQueue](
			OrderID [int] NOT NULL,
			OrderResponseID [int] NOT NULL,
			Valid bit not null default((1)),
      Amount decimal(18,2) not null ,
			CreationTime DateTime not null default((getdate()))
		
     CONSTRAINT PK_Refund_Queue PRIMARY KEY CLUSTERED 
    (
	    OrderID ASC,
	    OrderResponseID ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
    ) ON [PRIMARY]

      

      ALTER TABLE [dbo].RefundQueue  WITH CHECK ADD  CONSTRAINT [FK_RefundQueue_Order] FOREIGN KEY(OrderID)
      REFERENCES [dbo].[Order] (OrderID)

      ALTER TABLE [dbo].RefundQueue  WITH CHECK ADD  CONSTRAINT [FK_RefundQueue_OrderResponse] FOREIGN KEY(OrderResponseID)
      REFERENCES [dbo].[OrderResponse] (OrderResponseID)

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