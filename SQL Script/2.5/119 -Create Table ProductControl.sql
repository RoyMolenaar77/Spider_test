DECLARE @intErrorCode INT

BEGIN TRAN

  --Begin Action
	IF NOT EXISTS ( SELECT * FROM sys.tables WHERE NAME = 'ProductControl' )
		
		BEGIN
		
	CREATE TABLE [dbo].[ProductControl](
		[ProductControlID] [int] NOT NULL,
		[ProductControlName] [varchar](50) NOT NULL,
		[IsActive] [bit] NOT NULL,
		[ProductControlDescription] [varchar](250) NULL,
	CONSTRAINT [PK_ProductControle] PRIMARY KEY CLUSTERED 
	(
		[ProductControlID] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]


      PRINT 'Added table ProductControl'
    END
  ELSE
    BEGIN
      PRINT 'Table ProductControl already added to the database'
    END
  --End Action 

  SELECT @intErrorCode = @@ERROR
  IF (@intErrorCode <> 0)
    GOTO PROBLEM

COMMIT TRAN
PROBLEM:

IF (@intErrorCode <> 0)
BEGIN
  PRINT 'Unexpected error occurred while creating table ProductControl'

  ROLLBACK TRAN
END